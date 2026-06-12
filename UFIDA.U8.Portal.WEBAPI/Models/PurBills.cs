using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace UFIDA.U8.Portal.WEBAPI.Models
{
    public class PurBills
    {
        public string rdids { get; set; }

        public string cinvcode { get; set; }

        public decimal iquantity { get; set; }

        public decimal iOriCost { get; set; }

        public decimal iOriTaxCost { get; set; }

        public decimal iOriMoney { get; set; }

        public decimal iOriTaxPrice { get; set; }

        public decimal iOriSum { get; set; }

        public decimal iCost { get; set; }

        public decimal iMoney { get; set; }

        public decimal iTaxPrice { get; set; }

        public decimal iSum { get; set; }

        public decimal iTaxRate { get; set; }

        public string bExBill { get; set; }

        public string ivouchrowno { get; set; }
    }
}
