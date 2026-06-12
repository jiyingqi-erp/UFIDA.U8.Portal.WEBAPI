using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Net;
using System.Text;
using Nancy.Json;
using Newtonsoft.Json;

namespace UFIDA.U8.Portal.WEBAPI.DBhelp1
{
    public static class transform
    {
        public static DateTime GetTime(long timeStamp)
        {
            return new DateTime(1970, 1, 1, 8, 0, 0).AddMilliseconds(timeStamp);
        }

        public static int GetCreatetime()
        {
            DateTime dateTime = new DateTime(1970, 1, 1, 8, 0, 0);
            return Convert.ToInt32((DateTime.Now - dateTime).TotalSeconds);
        }

        public static string HttpApi(string url, string jsonstr, string type)
        {
            Encoding uTF = Encoding.UTF8;
            HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create(url);
            httpWebRequest.Accept = "text/html,application/xhtml+xml,*/*";
            httpWebRequest.ContentType = "application/json";
            httpWebRequest.Method = type.ToUpper().ToString();
            byte[] bytes = uTF.GetBytes(jsonstr);
            httpWebRequest.ContentLength = bytes.Length;
            httpWebRequest.GetRequestStream().Write(bytes, 0, bytes.Length);
            HttpWebResponse httpWebResponse = (HttpWebResponse)httpWebRequest.GetResponse();
            using StreamReader streamReader = new StreamReader(httpWebResponse.GetResponseStream(), Encoding.UTF8);
            return streamReader.ReadToEnd();
        }

        public static string Dtb2Json(DataTable dtb)
        {
            JavaScriptSerializer javaScriptSerializer = new JavaScriptSerializer();
            ArrayList arrayList = new ArrayList();
            foreach (DataRow row in dtb.Rows)
            {
                Dictionary<string, object> dictionary = new Dictionary<string, object>();
                foreach (DataColumn column in dtb.Columns)
                {
                    dictionary.Add(column.ColumnName, row[column.ColumnName]);
                }
                arrayList.Add(dictionary);
            }
            return javaScriptSerializer.Serialize(arrayList);
        }

        public static string DataTableToJsonWithJsonNet(DataTable table)
        {
            string empty = string.Empty;
            return JsonConvert.SerializeObject(table);
        }
    }
}
