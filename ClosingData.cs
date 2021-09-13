using System;
using System.Collections.Generic;
using System.Text;

namespace GetTradeClosingDataBatchJob
{
    public class ClosingData
    {
        public string StockCode { get; set; }
        public float ClosingPrice { get; set; }       
    }
}
