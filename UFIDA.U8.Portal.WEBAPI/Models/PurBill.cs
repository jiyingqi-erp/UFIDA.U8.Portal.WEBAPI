using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace UFIDA.U8.Portal.WEBAPI.Models
{
    public class PurBill
    {
        public string SRMID { get; set; }

        public string csource { get; set; }

        public string bNegative { get; set; }

        public string cInCode { get; set; }

        public string invoicetype { get; set; }

        public string invoicecode { get; set; }

        public string purchasecode { get; set; }

        public string ddate { get; set; }

        public string cvencode { get; set; }

        public string delegatecode { get; set; }

        public string cdepcode { get; set; }

        public string cpersoncode { get; set; }

        public string idiscounttaxtype { get; set; }

        public string payconditioncode { get; set; }

        public string cexch_name { get; set; }

        public string nflat { get; set; }

        public string Itaxrate { get; set; }

        public string cPBVMemo { get; set; }

        public string cBusType { get; set; }

        public string cPBVMaker { get; set; }

        public string cVenPUOMProtocol { get; set; }

        public List<PurBills> Items { get; set; }
    }
}