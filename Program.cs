using System;
using System.IO;
using log4net;
using System.Net.Http;
using System.Threading.Tasks;
using AngleSharp;
using System.Collections.Generic;
using System.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Data.SqlClient;
using System.ComponentModel;


// 這裡的Config，因為不能透過ConfigManager抓，使用相對路徑
[assembly: log4net.Config.XmlConfigurator(ConfigFile = @"..\..\..\log4net.config", Watch = true)]
namespace GetTradeClosingDataBatchJob
{
    class Program
    {
        private static ILog log = LogManager.GetLogger("logger");
        static async Task Main(string[] args)
        {
            string BatchJobName = ConfigurationManager.AppSettings["BatchJobName"].ToString();
            log.Info(string.Format("執行排程: {0}", BatchJobName));
            string StockCodes = ConfigurationManager.AppSettings["FollowStockCodes"].ToString();
            // 欲抓取的股票代號由Config設定
            IList<string> StockCodeList = new List<string>(StockCodes.Split(','));

            HttpClient httpClient = new HttpClient();
            string date = DateTime.Now.ToString("yyyyMMdd");
            
            string url = string.Format("https://www.twse.com.tw/exchangeReport/MI_INDEX?response=json&date={0}&type=ALLBUT0999", date);
            log.Info("開始發送請求");
            try
            {
                var responseMessage = await httpClient.GetAsync(url); //發送請求
               
                //檢查回應的伺服器狀態StatusCode是否是200 OK
                if (responseMessage.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    log.Info(string.Format("\n\tUrl: {0}\n\tStatusCode: {1}", url, responseMessage.StatusCode));

                    string responseResult = responseMessage.Content.ReadAsStringAsync().Result; //取得內容

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

                    /* 返回Json，進行Parse */
                    DataPrototype dataContainer = new DataPrototype();
                    dataContainer = JsonConvert.DeserializeObject<DataPrototype>(responseResult);
                    foreach (var Stock in dataContainer.data9)
                    {
                        if (StockCodes.IndexOf(Stock[0]) != -1)
                        {
                            ClosingData obj = new ClosingData{ 
                                StockCode = Stock[0],
                                ClosingPrice = float.Parse(Stock[8]) 
                            };
                            SqlExtend.ADODB.ExecuteNonQuery<ClosingData>(obj);
                        }
                    }
                }
                else
                {
                    log.Warn(string.Format("\n 請求失敗 Url:{0}\n\tStatusCode:{1}", url, responseMessage.StatusCode));
                }
            }
            catch(Exception ex)
            {
                log.Error(string.Concat("\n", ex.ToString()));
                log.Info(string.Format("執行中斷: {0}", BatchJobName));
                throw ex;
            }
            log.Info(string.Format("執行完畢: {0}", BatchJobName));
        }
    }
}
