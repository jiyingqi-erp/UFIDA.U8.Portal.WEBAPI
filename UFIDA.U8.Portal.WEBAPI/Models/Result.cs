using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UFIDA.U8.Portal.WEBAPI.Models
{
    public class Result
    {
        public string Code { get; set; }

        public string Msg { get; set; }

        public DataResult data { get; set; }
    }

}
