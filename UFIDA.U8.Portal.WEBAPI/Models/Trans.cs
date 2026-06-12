using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace UFIDA.U8.Portal.WEBAPI.Models
{
    public class Trans
    {
        public string cCode { get; set; }

        public string cSource { get; set; }

        public string RequestCode { get; set; }

        public string dDate { get; set; }

        public string cMaker { get; set; }

        public string cVerifier { get; set; }

        public string cODepCode { get; set; }

        public string cIDepCode { get; set; }

        public string cPersonCode { get; set; }

        public string cOWhCode { get; set; }

        public string cIWhCode { get; set; }

        public string cIRdCode { get; set; }

        public string cORdCode { get; set; }

        public string cMemo { get; set; }

        public List<TransItems> Items { get; set; }
    }
}