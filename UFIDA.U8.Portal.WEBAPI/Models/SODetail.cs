using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace UFIDA.U8.Portal.WEBAPI.Models
{
    public class SODetail
    {
        public string cType { get; set; }

        public int iRowNo { get; set; }

        public string cInvCode { get; set; }

        public string iQuantity { get; set; }

        public string dPreDate { get; set; }

        public string dPreMoDate { get; set; }

        public string iTaxRate { get; set; }

        public string iTaxUnitPrice { get; set; }

        public string cMemo { get; set; }

        public string cdefine22 { get; set; }

        public string cdefine23 { get; set; }
    }
}