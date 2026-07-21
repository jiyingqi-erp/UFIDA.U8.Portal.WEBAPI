using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace UFIDA.U8.Portal.WEBAPI.Models
{
    public class SaleBills
    {
        public string PatchCode { get; set; }

        public string PatchRow { get; set; }

        public string Rdids { get; set; }

        public string cInvCode { get; set; }

        public decimal iQuantity { get; set; }

        public string iTaxRate { get; set; }

        public string cMemo { get; set; }

        public string crmDetailId { get; set; }

        public int irowno { get; set; }
    }
}
