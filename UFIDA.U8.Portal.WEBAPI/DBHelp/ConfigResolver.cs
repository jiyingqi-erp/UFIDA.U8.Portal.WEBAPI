using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System;
using System.Configuration;

namespace UFIDA.U8.Portal.WEBAPI.DBHelp
{
    public class ConfigResolver
    {
        public static bool GETIP(string DB, out bool suc, out string result)
        {
            suc = false;
            result = string.Empty;
            try
            {
                string text = ConfigurationManager.AppSettings[DB];
                if (!string.IsNullOrEmpty(text))
                {
                    suc = true;
                    result = text;
                }
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
