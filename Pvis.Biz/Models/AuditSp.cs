using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace Pvis.Biz.Models
{
    /// <summary>勾稽比對_太陽光電板序號</summary>
    [Table("V_AuditSp")]
    public partial class AuditSp
    {
        /// <summary>太陽光電設備登記核准案件清冊(能源局匯入)_設備登記編號</summary>
        [Column("P_PVNo")]
        public string P_PVNo { get; set; }

        /// <summary>太陽光電設備登記核准案件清冊(能源局匯入)_申請人</summary>
        [Column("Applicant")]
        public string P_Applicant { get; set; }

        /// <summary>太陽光電設備登記核准案件清冊(能源局匯入)_太陽光電板序號</summary>
        [Column("P_sno")]
        public string P_sno { get; set; }

        /// <summary>設備登記資料_設備登記編號</summary>	
        [Column("U_pvno")]
        public string U_pvno { get; set; }

        /// <summary>帳號資料_公司名稱</summary>	
        [Column("CompanyName")]
        public string U_CompanyName { get; set; }

        /// <summary>設備登記資料_太陽光電板序號</summary>
        [Column("U_sno")]
        public string U_sno { get; set; }

        /// <summary>比對結果</summary>
        [NotMapped]
        public string AuditResult { 
            get {
                string AResult = "";
                if (P_Applicant != U_CompanyName)
                    AResult += "所有人不一致,";
                if (P_sno != U_sno)
                    AResult += "序號不一致";
                if ((P_Applicant == U_CompanyName) && (P_sno == U_sno))
                    AResult = "比對一致";
                return AResult;
            }
        }
    }
}
