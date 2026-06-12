using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace UFIDA.U8.Portal.WEBAPI.Models
{
    public class VouchsItem
    {
        public string VouchType { get; set; }

        public string VouchStatus { get; set; }

        public string VouchTimeS { get; set; }

        public string VouchTimeE { get; set; }

        public string cCode { get; set; }

        public string cHandler { get; set; }

        public string IsVerify { get; set; }

        public string cWhCode { get; set; }

        public string cWhName { get; set; }

        public string cBatch { get; set; }

        public string cPosCode { get; set; }

        public string Ver { get; set; }

        public string SolidWh { get; set; }

        public string cInvCode { get; set; }

        public string cInvCCode { get; set; }

        public string cInvName { get; set; }

        public string cInvStd { get; set; }
    }
}
