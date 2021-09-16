using System;
using System.IO;
using log4net;
using System.Net.Http;
using System.Threading.Tasks;
using AngleSharp;
using System.Collections.Generic;
using System.Configuration;
using System.Threading;
using Newtonsoft.Json;
using SqlExtend;



// 這裡的Config，因為不能透過ConfigManager抓，使用相對路徑
[assembly: log4net.Config.XmlConfigurator(ConfigFile = @"GetTradeClosingDataBatchJob.dll.config", Watch = true)]
namespace GetTradeClosingDataBatchJob
{
    class Program
    {
        #region 變數宣告

        private static ILog log = LogManager.GetLogger("logger");
        private static string StockCodes = ConfigurationManager.AppSettings["FollowStockCodes"].ToString();
        private static string BatchJobName = ConfigurationManager.AppSettings["BatchJobName"].ToString();
        private static string StartDate = ConfigurationManager.AppSettings["StartDate"].ToString();
        private static string EndDate = ConfigurationManager.AppSettings["EndDate"].ToString();

        #endregion

        /// <summary>
        /// 抓取每日盤後資訊
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        /// <history>
        /// 2021/09/15  Chris Liao  Create 
        /// </history>
        static void Main(string[] args)
        {
            
            log.Info(string.Format("執行排程: {0}", BatchJobName));

            // 欲抓取的股票代號由Config設定
            IList<string> StockCodeList = new List<string>(StockCodes.Split(','));

            /*
            if (!string.IsNullOrEmpty(StartDate))
            {
                if (!string.IsNullOrEmpty(EndDate))
                {
                    // StartDate及EndDate皆有值，抓取期間
                    // 取得區間日期
                    List<string> Interval = GetInterval(StartDate, EndDate);
                    foreach (var date in Interval)
                    {
                        Crawler(date);
                    }                   
                }
                else
                {
                    // 僅StartDate有值，抓取指定日期
                    Crawler(StartDate);
                }
            }
            else
            {
            */
                // StartDate及EndDate皆為空值，抓取當日
                string date = DateTime.Now.ToString("yyyyMMdd");
                Crawler(date);
            //}
            

            log.Info(string.Format("執行完畢: {0}", BatchJobName));
        }

        /// <summary>
        /// 爬蟲功能實作
        /// </summary>
        /// <param name="date"></param>
        /// <history>
        /// 2021/09/16  Chris Liao  Create 
        /// </history>
        static async void Crawler(string date)
        {
            HttpClient httpClient = new HttpClient();
            string url = string.Format("https://www.twse.com.tw/exchangeReport/MI_INDEX?response=json&date={0}&type=ALLBUT0999", date);
            log.Info(string.Format("開始發送請求\n\tUrl: {0}", url));
            try
            {
                Random rnd = new Random();
                //設置延遲
                Thread.Sleep(rnd.Next(1, 5));
                var responseMessage = await httpClient.GetAsync(url); //發送請求

                //檢查回應的伺服器狀態StatusCode是否是200 OK
                if (responseMessage.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    log.Info(string.Format("\n\tUrl: {0}\n\tStatusCode: {1}", url, responseMessage.StatusCode));

                    string responseResult = responseMessage.Content.ReadAsStringAsync().Result; //取得內容

                    /* 返回Json，進行Parse */
                    DataPrototype dataContainer = new DataPrototype();
                    dataContainer = JsonConvert.DeserializeObject<DataPrototype>(responseResult);
                    if (dataContainer.data9 != null && dataContainer.data9.Count > 0)
                    {
                        foreach (var Stock in dataContainer.data9)
                        {
                            if (StockCodes.IndexOf(Stock[0]) != -1)
                            {
                                ClosingData obj = new ClosingData
                                {
                                    StockCode = Stock[0],
                                    CompanyName = Stock[1],
                                    ClosingPrice = decimal.Round(decimal.Parse(Stock[8]), 2),
                                    Date = date,
                                    sys_createuser = "GetTradeClosingDataBatchJob"
                                };
                                ADODB.Insert<ClosingData>(obj);
                            }
                        }
                    }


                    /* 返回html，進行query
                    var config = AngleSharp.Configuration.Default;
                    var context = BrowsingContext.New(config);
                    var document = await context.OpenAsync(res => res.Content(responseResult));
                    //QuerySelector("head")找出<head></head>元素
                    var head = document.QuerySelector("head");
                    Console.WriteLine(head.ToHtml());
                    //QuerySelector(".entry-content")找出class="entry-content"的所有元素
                    var contents = document.QuerySelectorAll(".entry-content");

                    foreach (var c in contents)
                    {
                        //取得每個元素的TextContent
                        Console.WriteLine(c.TextContent);
                    }
                    */
                }
                else
                {
                    log.Warn(string.Format("\n 請求失敗 Url:{0}\n\tStatusCode:{1}", url, responseMessage.StatusCode));
                }
            }
            catch (Exception ex)
            {
                log.Error(string.Concat("\n", ex.ToString()));
                log.Info(string.Format("執行中斷: {0}", BatchJobName));
                throw ex;
            }
        }

        /// <summary>
        /// 取得日期區間，並排除六、日
        /// </summary>
        /// <param name="StartDate"></param>
        /// <param name="EndDate"></param>
        /// <returns></returns>
        /// <history>
        /// 2021/09/16  Chris Liao  Create 
        /// </history>
        static private List<string> GetInterval(string StartDate, string EndDate)
        {
            var yStart = int.Parse(StartDate.Substring(0, 4));
            var mStart = int.Parse(StartDate.Substring(4, 2));
            var dStart = int.Parse(StartDate.Substring(6, 2));
            var yEnd = int.Parse(EndDate.Substring(0, 4));
            var mEnd = int.Parse(EndDate.Substring(4, 2));
            var dEnd = int.Parse(EndDate.Substring(6, 2));
            List<string> returnInterval = new List<string>();

            for (var start = new DateTime(yStart, mStart, dStart); start <= new DateTime(yEnd, mEnd, dEnd); start = start.AddDays(1))
            {
                // 不等於六日則加入List
                if (start.DayOfWeek.ToString("d") != "6" && start.DayOfWeek.ToString("d") != "7")
                {
                    returnInterval.Add(start.ToString("yyyyMMdd"));
                }
            }

            return returnInterval;
        }
    }
}
