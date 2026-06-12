using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace UFIDA.U8.Portal.WEBAPI.Models
{
    public class PoDetails
    {
        public string cpoid { get; set; }

        public string vouchrowno { get; set; }

        public string cinvcode { get; set; }

        public string cinvversion { get; set; }

        public string cUnitID { get; set; }

        public decimal iquantity { get; set; }

        public string dArriveDate { get; set; }

        public decimal iUnitPrice { get; set; }

        public string iQuotedPrice { get; set; }

        public decimal iTaxPrice { get; set; }

        public decimal iMoney { get; set; }

        public decimal iTax { get; set; }

        public decimal iSum { get; set; }

        public string iDisCount { get; set; }

        public decimal iNatUnitPrice { get; set; }

        public decimal iNatMoney { get; set; }

        public string assistantunit { get; set; }

        public decimal iNatTax { get; set; }

        public decimal iNatSum { get; set; }

        public string iNatDisCount { get; set; }

        public decimal iPerTaxRate { get; set; }

        public string irowno { get; set; }
    }
}
