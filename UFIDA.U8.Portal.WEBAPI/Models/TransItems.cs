using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace UFIDA.U8.Portal.WEBAPI.Models
{
    public class TransItems
    {
        public string iTRIds { get; set; }

        public string cInvCode { get; set; }

        public string cBatch { get; set; }

        public string cbMemo { get; set; }

        public decimal iQuantity { get; set; }

        public string dMadeDate { get; set; }

        public string coPosCode { get; set; }

        public string ciPosCode { get; set; }
    }
}