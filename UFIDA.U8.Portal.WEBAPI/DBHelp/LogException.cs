using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UFIDA.U8.Portal.WEBAPI.DBHelp
{
    public class LogException
    {
        public static bool ilog = Convert.ToBoolean(ConfigurationManager.AppSettings["ilog"].ToString());

        private static string e = "";

        private static string eg = "";

        private static string sg = "";

        public static void WriteiLog(string filename, string srs, bool ilog, bool iilog)
        {
            if (ilog || iilog)
            {
                if (string.IsNullOrEmpty(filename))
                {
                    filename = "异常";
                }
                string text = AppDomain.CurrentDomain.SetupInformation.ApplicationBase + "\\log\\" + DateTime.Now.ToString("yyyy-MM-dd");
                if (!Directory.Exists(text))
                {
                    Directory.CreateDirectory(text);
                }
                string path = text + "\\" + DateTime.Now.ToString("yyyy-MM-dd") + "[" + filename + "]日志.log";
                StreamWriter streamWriter = new StreamWriter(path, append: true);
                streamWriter.WriteLine("当前时间：" + DateTime.Now.ToString());
                streamWriter.WriteLine("当前：" + srs);
                streamWriter.WriteLine();
                streamWriter.Close();
            }
        }

        public static void WriteLog(Exception ex, string sql)
        {
            string text = AppDomain.CurrentDomain.SetupInformation.ApplicationBase + "\\log\\" + DateTime.Now.ToString("yyyy-MM-dd");
            if (!Directory.Exists(text))
            {
                Directory.CreateDirectory(text);
            }
            string path = text + "\\" + DateTime.Now.ToString("yyyy-MM-dd") + "-异常日志.log";
            StreamWriter streamWriter = new StreamWriter(path, append: true);
            streamWriter.WriteLine("当前时间：" + DateTime.Now.ToString());
            streamWriter.WriteLine("异常信息：" + ex.Message);
            streamWriter.WriteLine("异常对象：" + ex.Source);
            streamWriter.WriteLine("调用堆栈：\n" + ex.StackTrace.Trim());
            streamWriter.WriteLine("触发语句：" + sql);
            streamWriter.WriteLine();
            streamWriter.Close();
            throw ex;
        }

        public static string WriteTxtLog(Exception ex)
        {
            string text = ex.ToString();
            e = text.Substring(0, text.IndexOf('语'));
            return e;
        }

        public static string WriteTxtEog(Exception ex)
        {
            string text = ex.ToString();
            eg = text.Substring(0, text.IndexOf('在'));
            return eg;
        }

        public static string WriteTxtSog(string cw)
        {
            sg = cw;
            return sg;
        }

        public static string exlog()
        {
            string result = ((!(e == "")) ? e : eg);
            if (sg != "")
            {
                result = sg;
            }
            return result;
        }

        public static void WriteLogts(string srs)
        {
            string text = AppDomain.CurrentDomain.SetupInformation.ApplicationBase + "\\log\\" + DateTime.Now.ToString("yyyy-MM-dd");
            if (!Directory.Exists(text))
            {
                Directory.CreateDirectory(text);
            }
            string path = text + "\\" + DateTime.Now.ToString("yyyy-MM-dd") + "-异常日志.log";
            StreamWriter streamWriter = new StreamWriter(path, append: true);
            streamWriter.WriteLine("当前时间：" + DateTime.Now.ToString());
            streamWriter.WriteLine("当前：" + srs);
            streamWriter.WriteLine();
            streamWriter.Close();
        }

        public static void WriteLogTZ(Exception ex, List<string> str)
        {
            string text = AppDomain.CurrentDomain.SetupInformation.ApplicationBase + "\\log\\" + DateTime.Now.ToString("yyyy-MM-dd");
            if (!Directory.Exists(text))
            {
                Directory.CreateDirectory(text);
            }
            string path = text + "\\" + DateTime.Now.ToString("yyyy-MM-dd") + "-异常日志.log";
            StreamWriter streamWriter = new StreamWriter(path, append: true);
            streamWriter.WriteLine("当前时间：" + DateTime.Now.ToString());
            streamWriter.WriteLine("异常信息：" + ex.Message);
            streamWriter.WriteLine("异常对象：" + ex.Source);
            streamWriter.WriteLine("调用堆栈：\n" + ex.StackTrace.Trim());
            streamWriter.WriteLine("触发语句：" + str.ToString());
            streamWriter.WriteLine();
            streamWriter.Close();
            throw ex;
        }
    }
}
