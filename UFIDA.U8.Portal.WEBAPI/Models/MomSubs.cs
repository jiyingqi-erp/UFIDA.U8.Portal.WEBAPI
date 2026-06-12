using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UFIDA.U8.Portal.WEBAPI.Models
{
    public class MomSubs
    {
        public string OpType { get; set; }

        public string cInvCode { get; set; }

        public decimal iQuantity { get; set; }

        public string cWhCode { get; set; }

        public string BaseQtyN { get; set; }

        public string BaseQtyD { get; set; }

        public string ParentScrap { get; set; }

        public string CompScrap { get; set; }
    }
}
