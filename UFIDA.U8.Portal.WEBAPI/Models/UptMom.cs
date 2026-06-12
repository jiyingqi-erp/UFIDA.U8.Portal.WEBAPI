using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UFIDA.U8.Portal.WEBAPI.Models
{
    public class UptMom
    {
        public string cCode { get; set; }

        public string cMaker { get; set; }

        public List<MomSubs> Subs { get; set; }
    }

}
