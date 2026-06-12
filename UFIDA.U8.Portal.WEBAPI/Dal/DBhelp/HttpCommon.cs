using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using UFIDA.U8.Portal.WEBAPI.DBhelp1;

namespace UFIDA.U8.Portal.WEBAPI.Dal.DBhelp
{
    public class HttpCommon
    {
        public static string CreatePostHttpResponseString(string url, string parameters, CookieCollection cookies)
        {
            try
            {
                HttpWebRequest httpWebRequest = null;
                httpWebRequest = ((!url.StartsWith("https", StringComparison.OrdinalIgnoreCase)) ? (WebRequest.Create(url) as HttpWebRequest) : (WebRequest.Create(url) as HttpWebRequest));
                httpWebRequest.Method = "POST";
                httpWebRequest.ContentType = "application/json;charset=utf-8";
                if (cookies != null)
                {
                    httpWebRequest.CookieContainer = new CookieContainer();
                    httpWebRequest.CookieContainer.Add(cookies);
                }
                byte[] bytes = Encoding.UTF8.GetBytes(parameters.ToString());
                using (Stream stream = httpWebRequest.GetRequestStream())
                {
                    stream.Write(bytes, 0, bytes.Length);
                }
                string[] values = httpWebRequest.Headers.GetValues("Content-Type");
                Stream responseStream = httpWebRequest.GetResponse().GetResponseStream();
                StreamReader streamReader = new StreamReader(responseStream, Encoding.GetEncoding("utf-8"));
                string result = streamReader.ReadToEnd();
                streamReader.Close();
                responseStream.Close();
                return result;
            }
            catch (Exception ex)
            {
                return ex.ToString();
            }
        }

        public static string CreateGetHttpResponse(string url, string entCode, string tokenId, CookieCollection cookies)
        {
            try
            {
                HttpWebRequest httpWebRequest = null;
                httpWebRequest = ((!url.StartsWith("https", StringComparison.OrdinalIgnoreCase)) ? (WebRequest.Create(url) as HttpWebRequest) : (WebRequest.Create(url) as HttpWebRequest));
                httpWebRequest.Method = "GET";
                httpWebRequest.ContentType = "application/json";
                httpWebRequest.Headers.Add("entCode", entCode);
                httpWebRequest.Headers.Add("tokenId", tokenId);
                HttpWebResponse httpWebResponse = (HttpWebResponse)httpWebRequest.GetResponse();
                return new StreamReader(httpWebResponse.GetResponseStream()).ReadToEnd();
            }
            catch (Exception ex)
            {
                return ex.ToString();
            }
        }

        public static string CreatePostHttpResponseStringIDictionary(string url, IDictionary<string, string> parameters, CookieCollection cookies)
        {
            HttpWebRequest httpWebRequest = null;
            httpWebRequest = ((!url.StartsWith("https", StringComparison.OrdinalIgnoreCase)) ? (WebRequest.Create(url) as HttpWebRequest) : (WebRequest.Create(url) as HttpWebRequest));
            httpWebRequest.Method = "POST";
            httpWebRequest.ContentType = "application/x-www-form-urlencoded;charset=utf-8";
            if (cookies != null)
            {
                httpWebRequest.CookieContainer = new CookieContainer();
                httpWebRequest.CookieContainer.Add(cookies);
            }
            if (parameters != null && parameters.Count != 0)
            {
                StringBuilder stringBuilder = new StringBuilder();
                int num = 0;
                foreach (string key in parameters.Keys)
                {
                    if (num > 0)
                    {
                        stringBuilder.AppendFormat("&{0}={1}", key, parameters[key]);
                        continue;
                    }
                    stringBuilder.AppendFormat("{0}={1}", key, parameters[key]);
                    num++;
                }
                stringBuilder.Replace("%", "%25");
                byte[] bytes = Encoding.UTF8.GetBytes(stringBuilder.ToString());
                using Stream stream = httpWebRequest.GetRequestStream();
                stream.Write(bytes, 0, bytes.Length);
            }
            string[] values = httpWebRequest.Headers.GetValues("Content-Type");
            Stream responseStream = httpWebRequest.GetResponse().GetResponseStream();
            StreamReader streamReader = new StreamReader(responseStream, Encoding.GetEncoding("utf-8"));
            string result = streamReader.ReadToEnd();
            streamReader.Close();
            responseStream.Close();
            return result;
        }

        public static string HttpPost(string url, string jsonParas, string token)
        {
            string empty = string.Empty;
            try
            {
                HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create(url);
                httpWebRequest.Method = "POST";
                httpWebRequest.ContentType = "application/json";
                if (token != "")
                {
                    WebHeaderCollection webHeaderCollection = new WebHeaderCollection();
                    webHeaderCollection.Add("token", token);
                    httpWebRequest.Headers = webHeaderCollection;
                }
                byte[] bytes = Encoding.UTF8.GetBytes(jsonParas);
                httpWebRequest.ContentLength = bytes.Length;
                Stream requestStream;
                try
                {
                    requestStream = httpWebRequest.GetRequestStream();
                }
                catch (Exception)
                {
                    return "null";
                }
                requestStream.Write(bytes, 0, bytes.Length);
                requestStream.Close();
                HttpWebResponse httpWebResponse;
                try
                {
                    httpWebResponse = (HttpWebResponse)httpWebRequest.GetResponse();
                }
                catch (WebException ex2)
                {
                    httpWebResponse = ex2.Response as HttpWebResponse;
                }
                Stream responseStream = httpWebResponse.GetResponseStream();
                StreamReader streamReader = new StreamReader(responseStream, Encoding.GetEncoding("utf-8"));
                empty = streamReader.ReadToEnd();
                streamReader.Close();
                responseStream.Close();
                return empty;
            }
            catch (Exception ex3)
            {
                DB.WriteTxt3("HttpGet:" + ex3.Message, "Http");
                return ex3.Message.ToString();
            }
        }

        public static string HttpGet(string Url, string postDataStr)
        {
            try
            {
                HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create(Url + ((postDataStr == "") ? "" : "?") + postDataStr);
                httpWebRequest.Method = "GET";
                httpWebRequest.ContentType = "text/html;charset=UTF-8";
                HttpWebResponse httpWebResponse = (HttpWebResponse)httpWebRequest.GetResponse();
                Stream responseStream = httpWebResponse.GetResponseStream();
                StreamReader streamReader = new StreamReader(responseStream, Encoding.UTF8);
                string result = streamReader.ReadToEnd();
                streamReader.Close();
                responseStream.Close();
                return result;
            }
            catch (Exception ex)
            {
                DB.WriteTxt3("HttpGet:" + ex.Message, "Http");
                return "false";
            }
        }

        public static string HttpPostFrom(string url, string data)
        {
            string result = "";
            try
            {
                HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create(url);
                httpWebRequest.Method = "POST";
                httpWebRequest.AllowAutoRedirect = true;
                httpWebRequest.Timeout = 20000;
                httpWebRequest.ContentType = "application/x-www-form-urlencoded";
                httpWebRequest.Headers.Add("x-cherun-auth-key", "LarxMbndsxfGwoYAqsfJSPPU42l04cb3");
                byte[] bytes = Encoding.Default.GetBytes(data);
                httpWebRequest.ContentLength = bytes.Length;
                using (Stream stream = httpWebRequest.GetRequestStream())
                {
                    stream.Write(bytes, 0, bytes.Length);
                    stream.Close();
                }
                HttpWebResponse httpWebResponse = (HttpWebResponse)httpWebRequest.GetResponse();
                Stream responseStream = httpWebResponse.GetResponseStream();
                using (StreamReader streamReader = new StreamReader(responseStream, Encoding.UTF8))
                {
                    result = streamReader.ReadToEnd();
                    responseStream.Close();
                }
                httpWebResponse.Close();
            }
            catch (Exception ex)
            {
                string message = ex.Message;
            }
            return result;
        }
    }
}
