using System;
using System.IO;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;

namespace UFIDA.U8.Portal.WEBAPI.DBHelp
{
    public class PMethod
    {
        public static string CutByteString(string str, int len)
        {
            string result = string.Empty;
            if (string.IsNullOrEmpty(str))
            {
                return result;
            }
            int byteCount = Encoding.Default.GetByteCount(str);
            int length = str.Length;
            int num = 0;
            int num2 = 0;
            if (byteCount > len)
            {
                for (int i = 0; i < length; i++)
                {
                    num = ((Convert.ToInt32(str.ToCharArray()[i]) <= 255) ? (num + 1) : (num + 3));
                    if (num > len)
                    {
                        num2 = i;
                        break;
                    }
                    if (num == len)
                    {
                        num2 = i + 1;
                        break;
                    }
                }
                if (num2 >= 0)
                {
                    result = str.Substring(0, num2);
                }
            }
            else
            {
                result = str;
            }
            return result;
        }

        public static string CutByteString(string str, int startIndex, int len)
        {
            string result = string.Empty;
            if (string.IsNullOrEmpty(str))
            {
                return result;
            }
            int byteCount = Encoding.Default.GetByteCount(str);
            int length = str.Length;
            if (startIndex == 0)
            {
                return CutByteString(str, len);
            }
            if (startIndex >= byteCount)
            {
                return result;
            }
            int num = startIndex + len;
            int num2 = 0;
            int num3 = 0;
            int num4 = 0;
            int num5 = 0;
            for (int i = 0; i < length; i++)
            {
                num2 = ((Convert.ToInt32(str.ToCharArray()[i]) <= 255) ? (num2 + 1) : (num2 + 2));
                if (num2 > startIndex)
                {
                    num4 = i;
                    num = startIndex + len - 1;
                    break;
                }
                if (num2 == startIndex)
                {
                    num4 = i + 1;
                    break;
                }
            }
            if (startIndex + len <= byteCount)
            {
                for (int j = 0; j < length; j++)
                {
                    num3 = ((Convert.ToInt32(str.ToCharArray()[j]) <= 255) ? (num3 + 1) : (num3 + 2));
                    if (num3 > num)
                    {
                        num5 = j;
                        break;
                    }
                    if (num3 == num)
                    {
                        num5 = j + 1;
                        break;
                    }
                }
                num5 -= num4;
            }
            else if (startIndex + len > byteCount)
            {
                num5 = length - num4;
            }
            if (num5 >= 0)
            {
                result = str.Substring(num4, num5);
            }
            return result;
        }

        public static string GetID()
        {
            byte[] array = Guid.NewGuid().ToByteArray();
            DateTime now = DateTime.Now;
            DateTime dateTime = new DateTime(1900, 1, 1);
            TimeSpan timeSpan = new TimeSpan(now.Ticks - dateTime.Ticks);
            TimeSpan timeSpan2 = new TimeSpan(now.Ticks - new DateTime(now.Year, now.Month, now.Day).Ticks);
            byte[] bytes = BitConverter.GetBytes(timeSpan.Days);
            byte[] bytes2 = BitConverter.GetBytes((long)(timeSpan2.TotalMilliseconds / 3.333333));
            Array.Copy(bytes, 0, array, 2, 2);
            Array.Copy(bytes2, 2, array, 0, 2);
            Array.Copy(bytes2, 0, array, 4, 2);
            return new Guid(array).ToString();
        }

        public static string StringTruncat(string oldStr, int maxLength, string endWith)
        {
            if (string.IsNullOrEmpty(oldStr))
            {
                return oldStr + endWith;
            }
            if (maxLength < 1)
            {
                throw new Exception("返回的字符串长度必须大于[0] ");
            }
            if (oldStr.Length > maxLength)
            {
                string text = oldStr.Substring(0, maxLength);
                if (string.IsNullOrEmpty(endWith))
                {
                    return text;
                }
                return text + endWith;
            }
            return oldStr;
        }

        public static string setvalue(int zdlx, string zd)
        {
            string result = zd;
            if (zdlx == 1 && string.IsNullOrEmpty(zd))
            {
                result = "0";
            }
            if (zdlx == 2)
            {
                result = ((!string.IsNullOrEmpty(zd)) ? ("'" + zd + "'") : "NULL");
            }
            return result;
        }

        public static long GetUnixTime(DateTime time)
        {
            return time.ToUniversalTime().Ticks / 10000000 - 62135596800L;
        }

        private int GetTimeStamp(DateTime dt)
        {
            DateTime dateTime = new DateTime(1970, 1, 1, 8, 0, 0);
            return Convert.ToInt32((dt - dateTime).TotalSeconds);
        }

        public static long DateTimeToTimestamp(DateTime datetime)
        {
            DateTime dateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0);
            DateTime dateTime2 = DateTime.SpecifyKind(datetime, DateTimeKind.Utc);
            return (long)(dateTime2 - dateTime).TotalMilliseconds;
        }

        private int ConvertDateTimeToInt32(string dt)
        {
            DateTime dateTime = new DateTime(1970, 1, 1, 8, 0, 0);
            DateTime dateTime2 = Convert.ToDateTime(dt);
            return Convert.ToInt32((dateTime2 - dateTime).TotalSeconds);
        }

        public static string CreatePostHttpResponses(string url, string parameters, CookieCollection cookies)
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
                return "false" + ex.Message;
            }
        }

        public static string CreatePostHttpResponse(string url, string parameters, string[] head, CookieCollection cookies)
        {
            HttpWebRequest httpWebRequest = null;
            httpWebRequest = ((!url.StartsWith("https", StringComparison.OrdinalIgnoreCase)) ? (WebRequest.Create(url) as HttpWebRequest) : (WebRequest.Create(url) as HttpWebRequest));
            httpWebRequest.Method = "POST";
            httpWebRequest.ContentType = "application/json;charset=utf-8";
            string text = head[0];
            string text2 = head[1];
            httpWebRequest.Headers.Add("entcode:" + text);
            httpWebRequest.Headers.Add("tokenID:" + text2);
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

        public static string MoneyToUpper(string LowerMoney)
        {
            string text = null;
            bool flag = false;
            if (LowerMoney.Trim().Substring(0, 1) == "-")
            {
                LowerMoney = LowerMoney.Trim().Remove(0, 1);
                flag = true;
            }
            string text2 = null;
            string text3 = null;
            string text4 = null;
            int num = 0;
            LowerMoney = Math.Round(double.Parse(LowerMoney), 2).ToString();
            if (LowerMoney.IndexOf(".") > 0)
            {
                if (LowerMoney.IndexOf(".") == LowerMoney.Length - 2)
                {
                    LowerMoney += "0";
                }
            }
            else
            {
                LowerMoney += ".00";
            }
            text2 = LowerMoney;
            num = 1;
            text4 = "";
            for (; num <= text2.Length; num++)
            {
                switch (text2.Substring(text2.Length - num, 1))
                {
                    case ".":
                        text3 = "圆";
                        break;
                    case "0":
                        text3 = "零";
                        break;
                    case "1":
                        text3 = "壹";
                        break;
                    case "2":
                        text3 = "贰";
                        break;
                    case "3":
                        text3 = "叁";
                        break;
                    case "4":
                        text3 = "肆";
                        break;
                    case "5":
                        text3 = "伍";
                        break;
                    case "6":
                        text3 = "陆";
                        break;
                    case "7":
                        text3 = "柒";
                        break;
                    case "8":
                        text3 = "捌";
                        break;
                    case "9":
                        text3 = "玖";
                        break;
                }
                text3 = num switch
                {
                    1 => text3 + "分",
                    2 => text3 + "角",
                    3 => text3 ?? "",
                    4 => text3 ?? "",
                    5 => text3 + "拾",
                    6 => text3 + "佰",
                    7 => text3 + "仟",
                    8 => text3 + "万",
                    9 => text3 + "拾",
                    10 => text3 + "佰",
                    11 => text3 + "仟",
                    12 => text3 + "亿",
                    13 => text3 + "拾",
                    14 => text3 + "佰",
                    15 => text3 + "仟",
                    16 => text3 + "万",
                    _ => text3 ?? "",
                };
                text4 = text3 + text4;
            }
            text4 = text4.Replace("零拾", "零");
            text4 = text4.Replace("零佰", "零");
            text4 = text4.Replace("零仟", "零");
            text4 = text4.Replace("零零零", "零");
            text4 = text4.Replace("零零", "零");
            text4 = text4.Replace("零角零分", "整");
            text4 = text4.Replace("零分", "整");
            text4 = text4.Replace("零角", "零");
            text4 = text4.Replace("零亿零万零圆", "亿圆");
            text4 = text4.Replace("亿零万零圆", "亿圆");
            text4 = text4.Replace("零亿零万", "亿");
            text4 = text4.Replace("零万零圆", "万圆");
            text4 = text4.Replace("零亿", "亿");
            text4 = text4.Replace("零万", "万");
            text4 = text4.Replace("零圆", "圆");
            text4 = text4.Replace("零零", "零");
            if (text4.Substring(0, 1) == "圆")
            {
                text4 = text4.Substring(1, text4.Length - 1);
            }
            if (text4.Substring(0, 1) == "零")
            {
                text4 = text4.Substring(1, text4.Length - 1);
            }
            if (text4.Substring(0, 1) == "角")
            {
                text4 = text4.Substring(1, text4.Length - 1);
            }
            if (text4.Substring(0, 1) == "分")
            {
                text4 = text4.Substring(1, text4.Length - 1);
            }
            if (text4.Substring(0, 1) == "整")
            {
                text4 = "零圆整";
            }
            text = text4;
            if (flag)
            {
                return "负" + text;
            }
            return text;
        }

        public static string ConvertToChinese(decimal number)
        {
            string input = number.ToString("#L#E#D#C#K#E#D#C#J#E#D#C#I#E#D#C#H#E#D#C#G#E#D#C#F#E#D#C#.0B0A");
            string input2 = Regex.Replace(input, "((?<=-|^)[^1-9]*)|((?'z'0)[0A-E]*((?=[1-9])|(?'-z'(?=[F-L\\.]|$))))|((?'b'[F-L])(?'z'0)[0A-L]*((?=[1-9])|(?'-z'(?=[\\.]|$))))", "${b}${z}");
            return Regex.Replace(input2, ".", (Match m) => "负元空零壹贰叁肆伍陆柒捌玖空空空空空空空分角拾佰仟万亿兆京垓秭穰"[m.Value[0] - 45].ToString());
        }

        public static int ToInt(string str)
        {
            try
            {
                return Convert.ToInt32(str);
            }
            catch
            {
                return 0;
            }
        }
    }
}
