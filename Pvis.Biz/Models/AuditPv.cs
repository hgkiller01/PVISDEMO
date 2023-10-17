using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace Pvis.Biz.Models
{
    /// <summary>勾稽比對_設備登記資料</summary>
    [Table("V_AuditPv")]
    public partial class AuditPv
    {
        /// <summary>太陽光電設備登記核准案件清冊(能源局匯入)_設備登記編號</summary>
        [Column("P_PVNo")]
        public string P_PVNo { get; set; }

        /// <summary>太陽光電設備登記核准案件清冊(能源局匯入)_申請人</summary>
        [Column("Applicant")]
        public string P_Applicant { get; set; }

        /// <summary>太陽光電設備登記核准案件清冊(能源局匯入)_設置場址(地址)</summary>
        [Column("P_PVAddr")]
        public string P_PVAddr { get; set; }

        /// <summary>太陽光電設備登記核准案件清冊(能源局匯入)_設備數量(片)</summary>
        [Column("P_SpQty")]
        public string P_SpQty { get; set; }

        /// <summary>設備登記資料_設備登記編號</summary>	
        [Column("U_pvno")]
        public string U_pvno { get; set; }

        /// <summary>帳號資料_公司名稱</summary>	
        [Column("CompanyName")]
        public string U_CompanyName { get; set; }

        /// <summary>設備登記資料_設置地址</summary>	
        [Column("U_pvaddr")]
        public string U_pvaddr { get; set; }

        /// <summary>設備登記資料_設備數量(片)</summary>
        [Column("U_SpQty")]
        public int U_SpQty { get; set; }

        /// <summary>比對結果</summary>
        [NotMapped]
        public string AuditResult { 
            get {
                string AResult = "";
                if (P_Applicant != U_CompanyName)
                    AResult += "所有人不一致,";
                if (P_PVAddr.Replace('台', '臺') != U_pvaddr)
                    AResult += "設置縣市不一致,";
                if (P_SpQty != U_SpQty.ToString())
                    AResult += "設備數量不一致";
                if ((P_Applicant == U_CompanyName) && (P_PVAddr.Replace('台', '臺') == U_pvaddr) && (P_SpQty == U_SpQty.ToString()))
                    AResult = "比對一致";
                return AResult;
            }
        }
    }
}
