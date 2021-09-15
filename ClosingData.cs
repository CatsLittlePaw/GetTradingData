using System;
using System.Collections.Generic;
using System.Text;

namespace GetTradeClosingDataBatchJob
{
    public class ClosingData
    {
        public string StockCode { get; set; }
        public string CompanyName { get; set; }
        public decimal ClosingPrice { get; set; }
        public string Date { get; set; }


        #region 資料庫必要欄位
        public DateTime? sys_createdate { get; set; }
        public string sys_createuser { get; set; }
        public DateTime? sys_updatedate { get; set; }
        public string sys_updateuser { get; set; }
        #endregion
    }
}
