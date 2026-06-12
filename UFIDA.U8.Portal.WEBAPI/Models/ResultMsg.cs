using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UFIDA.U8.Portal.WEBAPI.Models
{
    public class ResultMsg
    {
        public int statusCode { get; set; }

        public string ResultMessage { get; set; }

        public object Data { get; set; }

        public string Resp { get; set; }

        public ResultMsg SetResultMsg(int code, string msgs, object datas, string resps)
        {
            statusCode = code;
            ResultMessage = msgs;
            Data = datas;
            Resp = resps;
            return this;
        }

        internal ResultMsg SetResultMsg(int sUCCESS, object p, object Data, string resp)
        {
            throw new NotImplementedException();
        }
    }
}
