using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UFIDA.U8.Portal.WEBAPI.Models
{
    public class DataResult
    {
        public string U8Code { get; set; }

        public string U8ID { get; set; }

        public List<ItemsItem> Items { get; set; }
    }
}
