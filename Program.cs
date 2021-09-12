using System;
using System.IO;
using log4net;
using log4net.Config;
using System.Net.Http;
using System.Configuration;

// 這裡的Config，因為不能透過ConfigManager抓，使用相對路徑
[assembly: log4net.Config.XmlConfigurator(ConfigFile = @"..\..\..\log4net.config", Watch = true)]
namespace GetTradeClosingDataBatchJob
{
    class Program
    {
        private static ILog log = LogManager.GetLogger(typeof(Program));        
        static void Main(string[] args)
        {            

        }
    }
}
