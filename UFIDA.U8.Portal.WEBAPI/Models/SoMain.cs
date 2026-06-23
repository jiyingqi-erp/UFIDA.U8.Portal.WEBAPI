using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace UFIDA.U8.Portal.WEBAPI.Models
{
    public class SoMain
    {
        public string cSoCode { get; set; }

        public string cCusCode { get; set; }

        public string cChanger { get; set; }

        public string cPersonCode { get; set; }

        public string cexch_name { get; set; }

        public decimal iExchRate { get; set; }

        public string cMemo { get; set; }

        public List<SODetail> Items { get; set; }
    }

}