using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UFIDA.U8.Portal.WEBAPI.DBhelp1
{
    public class LogException
    {
        private static string e = "";

        public static void WriteLog(Exception ex, string sql)
        {
            string text = AppDomain.CurrentDomain.SetupInformation.ApplicationBase + "\\log\\异常日志";
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
            streamWriter.Dispose();
            streamWriter.Close();
            throw ex;
        }

        public static void WriteLog(string sql)
        {
            string text = AppDomain.CurrentDomain.SetupInformation.ApplicationBase + "\\log\\异常日志";
            if (!Directory.Exists(text))
            {
                Directory.CreateDirectory(text);
            }
            string path = text + "\\" + DateTime.Now.ToString("yyyy-MM-dd") + "-SQL日志.log";
            StreamWriter streamWriter = new StreamWriter(path, append: true);
            streamWriter.WriteLine("当前时间：" + DateTime.Now.ToString());
            streamWriter.WriteLine("触发语句：" + sql);
            streamWriter.WriteLine();
            streamWriter.Dispose();
            streamWriter.Close();
        }

        public static void WriteTxt(string LogType, string Json, string Method)
        {
            DateTime dateTime = default(DateTime);
            dateTime = DateTime.Now;
            string text = dateTime.Year + dateTime.Month.ToString().PadLeft(2, '0');
            string text2 = LogType + "_" + Method + "_" + dateTime.ToString("yyyy-MM-dd") + ".txt";
            string text3 = AppDomain.CurrentDomain.SetupInformation.ApplicationBase + "\\log\\" + LogType + "\\" + text;
            if (!Directory.Exists(text3))
            {
                Directory.CreateDirectory(text3);
            }
            string path = text3 + "\\" + text2;
            if (!File.Exists(path))
            {
                using (StreamWriter streamWriter = File.CreateText(path))
                {
                    streamWriter.WriteLine("/******************************************************************/");
                    streamWriter.WriteLine("当前时间：" + DateTime.Now.ToString());
                    streamWriter.WriteLine("信息：" + Json);
                    streamWriter.WriteLine("/******************************************************************/");
                    streamWriter.Dispose();
                    streamWriter.Close();
                    return;
                }
            }
            StreamReader streamReader = File.OpenText(path);
            string value = streamReader.ReadToEnd();
            streamReader.Close();
            File.Delete(path);
            using StreamWriter streamWriter2 = new StreamWriter(path);
            streamWriter2.WriteLine(value);
            streamWriter2.WriteLine("/******************************************************************/");
            streamWriter2.WriteLine("当前时间：" + DateTime.Now.ToString());
            streamWriter2.WriteLine("信息：" + Json);
            streamWriter2.WriteLine("/******************************************************************/");
            streamWriter2.Dispose();
            streamWriter2.Close();
        }

        public static void WriteJSlog(string LogType, string Json, string ResultJson)
        {
            DateTime dateTime = default(DateTime);
            dateTime = DateTime.Now;
            string text = dateTime.Year + dateTime.Month.ToString().PadLeft(2, '0');
            string text2 = LogType + "_" + dateTime.ToString("yyyy-MM-dd") + ".txt";
            string text3 = AppDomain.CurrentDomain.SetupInformation.ApplicationBase + "\\log\\" + LogType + "\\" + text;
            if (!Directory.Exists(text3))
            {
                Directory.CreateDirectory(text3);
            }
            string path = text3 + "\\" + text2;
            if (!File.Exists(path))
            {
                using (StreamWriter streamWriter = File.CreateText(path))
                {
                    streamWriter.WriteLine("/******************************************************************/");
                    streamWriter.WriteLine("当前时间：" + DateTime.Now.ToString());
                    streamWriter.WriteLine("接收信息：" + Json);
                    streamWriter.WriteLine("--------------------------------------------------------------------");
                    streamWriter.WriteLine("返回信息：" + ResultJson);
                    streamWriter.WriteLine("/******************************************************************/");
                    streamWriter.Dispose();
                    streamWriter.Close();
                    return;
                }
            }
            string value = "";
            using (StreamReader streamReader = File.OpenText(path))
            {
                value = streamReader.ReadToEnd();
                streamReader.Close();
            }
            File.Delete(path);
            using StreamWriter streamWriter2 = new StreamWriter(path);
            streamWriter2.WriteLine(value);
            streamWriter2.WriteLine("/******************************************************************/");
            streamWriter2.WriteLine("当前时间：" + DateTime.Now.ToString());
            streamWriter2.WriteLine("接收信息：" + Json);
            streamWriter2.WriteLine("--------------------------------------------------------------------");
            streamWriter2.WriteLine("返回信息：" + ResultJson);
            streamWriter2.WriteLine("/******************************************************************/");
            streamWriter2.Dispose();
            streamWriter2.Close();
        }

        public static void WriteTxt(string LogType, string Json, string Method, string VouchType)
        {
            DateTime dateTime = default(DateTime);
            dateTime = DateTime.Now;
            string text = dateTime.Year + dateTime.Month.ToString().PadLeft(2, '0');
            string text2 = LogType + "_" + Method + "_" + VouchType + "_" + dateTime.ToString("yyyy-MM-dd") + ".txt";
            string text3 = AppDomain.CurrentDomain.SetupInformation.ApplicationBase + "\\log\\" + LogType + "\\" + text;
            if (!Directory.Exists(text3))
            {
                Directory.CreateDirectory(text3);
            }
            string path = text3 + "\\" + text2;
            if (!File.Exists(path))
            {
                using (StreamWriter streamWriter = File.CreateText(path))
                {
                    streamWriter.WriteLine("/******************************************************************/");
                    streamWriter.WriteLine("当前时间：" + DateTime.Now.ToString());
                    streamWriter.WriteLine("信息：" + Json);
                    streamWriter.WriteLine("/******************************************************************/");
                    streamWriter.Dispose();
                    streamWriter.Close();
                    return;
                }
            }
            string value = "";
            using (StreamReader streamReader = File.OpenText(path))
            {
                value = streamReader.ReadToEnd();
                streamReader.Close();
            }
            File.Delete(path);
            using StreamWriter streamWriter2 = new StreamWriter(path);
            streamWriter2.WriteLine(value);
            streamWriter2.WriteLine("/******************************************************************/");
            streamWriter2.WriteLine("当前时间：" + DateTime.Now.ToString());
            streamWriter2.WriteLine("信息：" + Json);
            streamWriter2.WriteLine("/******************************************************************/");
            streamWriter2.Dispose();
            streamWriter2.Close();
        }

        public static void WriteException(Exception ex, string Json)
        {
            DateTime dateTime = default(DateTime);
            dateTime = DateTime.Now;
            string text = dateTime.Year + dateTime.Month.ToString().PadLeft(2, '0');
            string text2 = dateTime.ToString("yyyy-MM-dd") + ".txt";
            string path = AppDomain.CurrentDomain.SetupInformation.ApplicationBase + "\\log\\异常日志\\" + text + "\\" + text2;
            if (!File.Exists(path))
            {
                using (StreamWriter streamWriter = File.CreateText(path))
                {
                    streamWriter.WriteLine("/******************************************************************/");
                    streamWriter.WriteLine("当前时间：" + DateTime.Now.ToString());
                    streamWriter.WriteLine("信息：" + Json);
                    streamWriter.WriteLine("/******************************************************************/");
                }
            }
        }
    }
}
