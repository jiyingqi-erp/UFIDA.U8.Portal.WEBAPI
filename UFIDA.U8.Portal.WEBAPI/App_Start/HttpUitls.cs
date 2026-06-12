using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;

namespace UFIDA.U8.Portal.WEBAPI
{
    public class HttpUitls
    {
        public static string Get(string Url)
        {
            HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create(Url);
            httpWebRequest.Proxy = null;
            httpWebRequest.KeepAlive = false;
            httpWebRequest.Method = "GET";
            httpWebRequest.ContentType = "application/json; charset=UTF-8";
            httpWebRequest.AutomaticDecompression = DecompressionMethods.GZip;
            HttpWebResponse httpWebResponse = (HttpWebResponse)httpWebRequest.GetResponse();
            Stream responseStream = httpWebResponse.GetResponseStream();
            StreamReader streamReader = new StreamReader(responseStream, Encoding.UTF8);
            string result = streamReader.ReadToEnd();
            streamReader.Close();
            responseStream.Close();
            httpWebResponse?.Close();
            httpWebRequest?.Abort();
            return result;
        }

        public static string Post(string Url, string Data, string Referer)
        {
            HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create(Url);
            httpWebRequest.Method = "POST";
            httpWebRequest.Referer = Referer;
            byte[] bytes = Encoding.UTF8.GetBytes(Data);
            httpWebRequest.ContentType = "application/x-www-form-urlencoded";
            httpWebRequest.ContentLength = bytes.Length;
            Stream requestStream = httpWebRequest.GetRequestStream();
            requestStream.Write(bytes, 0, bytes.Length);
            HttpWebResponse httpWebResponse = (HttpWebResponse)httpWebRequest.GetResponse();
            StreamReader streamReader = new StreamReader(httpWebResponse.GetResponseStream(), Encoding.UTF8);
            string result = streamReader.ReadToEnd();
            streamReader.Close();
            requestStream.Close();
            httpWebResponse?.Close();
            httpWebRequest?.Abort();
            return result;
        }
    }
}