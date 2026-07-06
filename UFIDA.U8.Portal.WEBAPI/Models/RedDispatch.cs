using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace UFIDA.U8.Portal.WEBAPI.Models
{
    public class RedDispatch
    {
        public string cCode { get; set; }

        public string cMaker { get; set; }

        public string cVerifier { get; set; }

        public string dDate { get; set; }

        public string cDepCode { get; set; }

        public string cPersonCode { get; set; }

        public string bsaleoutcreatebill { get; set; }

        public string cMemo { get; set; }

        public string cDefine13 { get; set; }

        public List<RedDispatchs> Items { get; set; }
    }

}