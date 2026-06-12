using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace UFIDA.U8.Portal.WEBAPI.Models
{
    public class PriceJustDetails
    {
        public string cInvCode { get; set; }

        public string cVenCode { get; set; }

        public string cexch_name { get; set; }

        public string fminquantity { get; set; }

        public string iTaxUnitPrice { get; set; }

        public string iUnitPrice { get; set; }

        public string iTaxRate { get; set; }

        public string dstartdate { get; set; }

        public string denddate { get; set; }

        public string cbMemo { get; set; }

        public string cDefine23 { get; set; }

        public string fMinSuppNum { get; set; }

        public string fSupplyBatch { get; set; }

        public string fAdvDate { get; set; }
    }

}
