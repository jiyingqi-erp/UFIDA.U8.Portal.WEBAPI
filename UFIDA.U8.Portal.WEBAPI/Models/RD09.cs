using System.Collections.Generic;

namespace UFIDA.U8.Portal.WEBAPI.Models
{
    public class RD09
    {
        public string cType { get; set; }

        public string dDate { get; set; }

        public string cCode { get; set; }

        public string cMaker { get; set; }

        public string cVerifier { get; set; }

        public string cDepCode { get; set; }

        public string cPersonCode { get; set; }

        public string cWhCode { get; set; }

        public string cRdCode { get; set; }

        public string cMemo { get; set; }

        public List<RDs09> Items { get; set; }
    }
}
