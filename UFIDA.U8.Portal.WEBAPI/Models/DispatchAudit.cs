using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace UFIDA.U8.Portal.WEBAPI.Models
{
    /// <summary>
    /// 销售发货单审核请求模型
    /// 请求JSON格式: {"consignment":{"voucher_code":"XSFH-26073888","person_code":"444"}}
    /// </summary>
    public class DispatchAudit
    {
        /// <summary>
        /// 发货单数据
        /// </summary>
        public DispatchAuditInner consignment { get; set; }
    }

    public class DispatchAuditInner
    {
        /// <summary>
        /// 发货单号（DispatchList.cDLCode）
        /// </summary>
        public string voucher_code { get; set; }

        /// <summary>
        /// 审核人员工编码（对应 person.cPersonCode）
        /// </summary>
        public string person_code { get; set; }
    }
}
