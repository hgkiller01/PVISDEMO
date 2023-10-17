using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Pvis.Biz.CustomizedAttr;

namespace Pvis.Biz.Models
{
    /// <summary>清理行程表-審查確認</summary>
    [Table("Schedule_Review", Schema = "Apply")]
    public partial class Schedule_Review : ScheduleBase
    {
        /// <summary>申請id</summary>	
        [Key]
        [Column("pid")]
        [Display(Name = "申請id")]
        public int Pid { get; set; }

        /// <summary>清理編號(C+民國年+月+日+3碼流水號)</summary>	
        [Column("Cle_Sch_No")]
        [Display(Name = "清理編號(C+民國年+月+日+3碼流水號)")]
        [StringLength(11)]
        [NeverUpdate]
        public string Cle_Sch_No { get; set; }

    }
}
