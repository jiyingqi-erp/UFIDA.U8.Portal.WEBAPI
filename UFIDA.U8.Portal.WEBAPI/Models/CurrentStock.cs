using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace UFIDA.U8.Portal.WEBAPI.Models
{
    public class CurrentStock
    {
        public string AutoID { get; set; }

        public string InvCode { get; set; }

        public string InvName { get; set; }

        public string InvStd { get; set; }


        public string WhCode { get; set; }

        public string WhName { get; set; }

        public string ComUnitCode { get; set; }


        public decimal iQuantity { get; set; }

    }
}
