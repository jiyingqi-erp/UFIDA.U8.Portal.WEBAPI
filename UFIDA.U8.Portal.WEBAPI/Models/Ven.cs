using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace UFIDA.U8.Portal.WEBAPI.Models
{
    public class Ven
    {
        public string CVenCode { get; set; }

        public decimal fQuota { get; set; }

        public string MainVen { get; set; }

        public string fMinSuppNum { get; set; }

        public string fSupplyBatch { get; set; }

        public string fAdvDate { get; set; }
    }
}