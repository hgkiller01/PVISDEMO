using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Pvis.Biz.CustomizedAttr;

namespace Pvis.Biz.Models
{
    /// <summary>稽核行程表-審查確認</summary>
    [Table("ScheduleAudit_Review", Schema = "Apply")]
    public partial class ScheduleAudit_Review : ScheduleBase
    {
        /// <summary>申請id</summary>	
        [Key]
        [Column("pid")]
        [Display(Name = "申請id")]
        public int Pid { get; set; }

        /// <summary>稽核編號(A+民國年+月+日+3碼流水號)</summary>	
        [Column("Aud_Sch_No")]
        [Display(Name = "稽核編號(A+民國年+月+日+3碼流水號)")]
        [StringLength(11)]
        [NeverUpdate]
        public string Aud_Sch_No { get; set; }

        /// <summary>允收稽核人員名單</summary>
        [StringLength(100)]
        public string Aud_Man { get; set; }

        /// <summary>允收稽核預計停留時間(分鐘)</summary>
        [Column("Pre_Minute")]
        [Display(Name = "允收稽核預計停留時間(分鐘)")]
        public int? Pre_Minute { get; set; }

        /// <summary>處理稽核人員名單</summary>
        [StringLength(100)]
        public string Tre_Aud_Man { get; set; }

        /// <summary>處理稽核預計停留時間(分鐘)</summary>
        [Column("Tre_Pre_Minute")]
        [Display(Name = "處理稽核預計停留時間(分鐘)")]
        public int? Tre_Pre_Minute { get; set; }
    }
}
