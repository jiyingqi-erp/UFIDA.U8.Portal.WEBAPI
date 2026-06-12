using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UFIDA.U8.Portal.WEBAPI.Models
{
    public class MomOrders
    {
        public string U8ID { get; set; }

        public string cInvCode { get; set; }

        public decimal iQuantity { get; set; }

        public string MoLotCode { get; set; }

        public string cDepCode { get; set; }

        public string StartDate { get; set; }

        public string DueDate { get; set; }

        public string Remark { get; set; }

        public string cItem_class { get; set; }

        public string cItemCode { get; set; }

        public string cItemName { get; set; }

        public List<MomSubs> Subs { get; set; }

        public string cdefine23 { get; set; }
    }
}
