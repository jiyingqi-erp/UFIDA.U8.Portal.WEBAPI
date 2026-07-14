using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace UFIDA.U8.Portal.WEBAPI.Models
{
    public class SaleBillInvoice
    {
        /// <summary>
        /// 发票单据号（cSBVCode）
        /// </summary>
        public string cSBVCode { get; set; }

        /// <summary>
        /// 已开发票号（写入cDefine12）
        /// </summary>
        public string cInvoiceNo { get; set; }
    }
}
