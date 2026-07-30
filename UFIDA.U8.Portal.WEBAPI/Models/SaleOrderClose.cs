using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace UFIDA.U8.Portal.WEBAPI.Models
{
    /// <summary>
    /// 销售订单关闭请求模型
    /// 请求JSON格式: {"saleorder":{"voucher_code":"0000000019","person_code":"00002"}}
    /// </summary>
    public class SaleOrderClose
    {
        /// <summary>
        /// 销售订单数据
        /// </summary>
        public SaleOrderCloseInner saleorder { get; set; }
    }

    public class SaleOrderCloseInner
    {
        /// <summary>
        /// 销售订单号（SO_SOMain.CSOCODE）
        /// </summary>
        public string voucher_code { get; set; }

        /// <summary>
        /// 关闭人员工编码（对应 person.cPersonCode）
        /// </summary>
        public string person_code { get; set; }
    }
}
