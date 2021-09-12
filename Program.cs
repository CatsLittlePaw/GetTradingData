using System;
using System.IO;
using log4net;
using System.Net.Http;
using System.Threading.Tasks;
using AngleSharp;


// 這裡的Config，因為不能透過ConfigManager抓，使用相對路徑
[assembly: log4net.Config.XmlConfigurator(ConfigFile = @"..\..\..\log4net.config", Watch = true)]
namespace GetTradeClosingDataBatchJob
{
    class Program
    {
        private static ILog log = LogManager.GetLogger(typeof(Program));        
        static async Task Main(string[] args)
        {
            HttpClient httpClient = new HttpClient();

            string url = "https://dannyliu.me";
            var responseMessage = await httpClient.GetAsync(url); //發送請求

            //檢查回應的伺服器狀態StatusCode是否是200 OK
            if (responseMessage.StatusCode == System.Net.HttpStatusCode.OK)
            {
                string responseResult = responseMessage.Content.ReadAsStringAsync().Result;//取得內容

                Console.WriteLine(responseResult);
                
                var config = Configuration.Default;
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
                
            }
        }
    }
}
