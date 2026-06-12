using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace UFIDA.U8.Portal.WEBAPI.Models
{
        public class Pomain
        {
            public string ccode { get; set; }

            public string Ddate { get; set; }

            public string cBusType { get; set; }

            public string state { get; set; }

            public string cPTCode { get; set; }

            public string cvencode { get; set; }

            public string cdepcode { get; set; }

            public string cpersoncode { get; set; }

            public string cmaker { get; set; }

            public string cverifier { get; set; }

            public string closer { get; set; }

            public string cexch_name { get; set; }

            public string nflat { get; set; }

            public string remark { get; set; }

            public string dArriveDate { get; set; }

            public string cbCloseDate { get; set; }

            public string cbCloser { get; set; }

            public string irowno { get; set; }

            public string cDefine14 { get; set; }

            public List<PoDetails> Items { get; set; }
        }
    }
