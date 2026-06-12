using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UFIDA.U8.Portal.WEBAPI.DBHelp
{
    public class InventoryValidationResult
    {
        public int ErrorCount { get; set; } = 0;

        public string ErrorMessage { get; set; } = string.Empty;

        public string WhereCondition { get; set; } = string.Empty;
    }
}
