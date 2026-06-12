using System;
using System.Collections.Generic;

namespace UFIDA.U8.Portal.WEBAPI.Models
{
    public class PuArr
    {
        public string cType { get; set; }

        public string cBusType { get; set; }

        public string cCode { get; set; }

        public DateTime dDate { get; set; }

        public string cMaker { get; set; }

        public string cHandler { get; set; }

        public string cDepCode { get; set; }

        public string cMemo { get; set; }

        public List<PuArrs> Items { get; set; }
    }
}
