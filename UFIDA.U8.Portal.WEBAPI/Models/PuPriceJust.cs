using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace UFIDA.U8.Portal.WEBAPI.Models
{
    public class PuPriceJust
    {
        public string cCode { get; set; }

        public string cMaker { get; set; }

        public string dDate { get; set; }

        public string iSupplyType { get; set; }

        public string cDepCode { get; set; }

        public string cMemo { get; set; }

        public List<PriceJustDetails> Items { get; set; }
    }
}
