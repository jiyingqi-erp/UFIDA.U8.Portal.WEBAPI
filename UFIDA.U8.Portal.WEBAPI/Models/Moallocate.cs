using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UFIDA.U8.Portal.WEBAPI.Models
{
    public class Moallocate
    {
        public string cType { get; set; }

        public string U8ID { get; set; }

        public string cMaker { get; set; }

        public string cInvCode { get; set; }

        public List<Moallocates> Items { get; set; }
    }
}
