using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UFIDA.U8.Portal.WEBAPI.Models
{
    public class MomOrder
    {
        public string cType { get; set; }

        public string cCode { get; set; }

        public string cMaker { get; set; }

        public string MoClass { get; set; }

        public string U8Code { get; set; }

        public string CloseUser { get; set; }

        public string CloseTime { get; set; }

        public List<MomOrders> Items { get; set; }
    }

}
