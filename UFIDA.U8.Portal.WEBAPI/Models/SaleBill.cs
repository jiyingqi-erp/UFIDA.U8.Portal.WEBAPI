using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UFIDA.U8.Portal.WEBAPI.Models
{
    public class SaleBill
    {
        public string cVouchType { get; set; }

        public string cSBVCode { get; set; }

        public string cMaker { get; set; }

        public string cVerifier { get; set; }

        public string dDate { get; set; }

        public string cDepCode { get; set; }

        public string cPersonCode { get; set; }

        public string cMemo { get; set; }

        public List<SaleBills> Items { get; set; }
    }


}
