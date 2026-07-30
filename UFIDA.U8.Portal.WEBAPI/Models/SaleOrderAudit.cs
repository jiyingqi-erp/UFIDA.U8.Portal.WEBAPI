using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace UFIDA.U8.Portal.WEBAPI.Models
{
    /// <summary>
    /// 销售订单审核请求模型
    /// 请求JSON格式: {"saleorder":{"voucher_code":"SO#20260616-0003","person_code":"157"}}
    /// </summary>
    public class SaleOrderAudit
    {
        /// <summary>
        /// 销售订单数据
        /// </summary>
        public SaleOrderAuditInner saleorder { get; set; }
    }

    public class SaleOrderAuditInner
    {
        /// <summary>
        /// 销售订单号（SO_SOMain.CSOCODE）
        /// </summary>
        public string voucher_code { get; set; }

        /// <summary>
        /// 审核人员工编码（对应 person.cPersonCode）
        /// </summary>
        public string person_code { get; set; }
    }
}
