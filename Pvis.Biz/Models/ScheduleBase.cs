using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Pvis.Biz.CustomizedAttr;
using System.Text;

namespace Pvis.Biz.Models
{
    /// <summary>
    /// 清理 稽核共用欄位
    /// </summary>
    public class ScheduleBase
    {
        /// <summary>清理行程表-審查確認 確認狀態(1:已安排日期,D:待確認合約,D1:已確認合約,DM:合約須補正,C1:已完成清理)</summary>
        /// <summary>稽核行程表-審查確認 確認狀態(1:填寫中,S:提出申請,M:待補正,Y1:通過)</summary>
        [Required(ErrorMessage = "『確認狀態』未填寫")]
        [StringLength(2)]
        public string Status { get; set; }

        /// <summary>申請日期</summary>	
        [Column("App_Date", TypeName = "datetime")]
        [Display(Name = "申請日期")]
        [NeverUpdate]
        public DateTime App_Date { get; set; }

        /// <summary>確認者代碼</summary>
        public int Uid { get; set; }

        /// <summary>確認者名稱</summary>
        public string Check_Name { get; set; }

        /// <summary>審核日期</summary>	
        [Column("Check_Date", TypeName = "datetime")]
        [Display(Name = "審核日期")]
        [NeverUpdate]
        public DateTime Check_Date { get; set; }

        /// <summary>意見</summary>
        [StringLength(100)]
        public string Check_opinion { get; set; }
    }
}
