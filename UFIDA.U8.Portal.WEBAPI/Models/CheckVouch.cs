using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UFIDA.U8.Portal.WEBAPI.Models
{
    public class CheckVouch
    {
        public string ccvcode { get; set; }

        public string dcvdate { get; set; }

        public string dacdate { get; set; }

        public string cwhcode { get; set; }

        public string ccvmemo { get; set; }

        public string cmaker { get; set; }

        public string caccounter { get; set; }

        public List<Items> Items { get; set; }
    }

}
