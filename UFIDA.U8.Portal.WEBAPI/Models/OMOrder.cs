using System.Collections.Generic;

namespace UFIDA.U8.Portal.WEBAPI.Models
{
    public class OMOrder
    {
        public string cCode { get; set; }

        public string dDate { get; set; }

        public string cVenCode { get; set; }

        public string cexch_name { get; set; }

        public string nflat { get; set; }

        public string cMaker { get; set; }

        public string cVerifier { get; set; }

        public string cPersonCode { get; set; }

        public string cDepCode { get; set; }

        public string cMemo { get; set; }

        public List<OMDetail> Items { get; set; }
    }
}
