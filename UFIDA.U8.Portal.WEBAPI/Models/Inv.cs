using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace UFIDA.U8.Portal.WEBAPI.Models
{
    public class Inv
    {
        public string iSuppProperty { get; set; }

        public string cInvCode { get; set; }

        public List<Ven> Vens { get; set; }
    }
}