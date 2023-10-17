using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Pvis.Biz.CustomizedAttr;

namespace Pvis.Biz.Models
{
    /// <summary>稽核行程表</summary>
    [Table("ScheduleAudit", Schema = "Apply")]
    public partial class ScheduleAudit
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

        /// <summary>申請日期</summary>	
        [Column("App_Date", TypeName = "datetime")]
        [Display(Name = "申請日期")]
        [NeverUpdate]
        public DateTime App_Date { get; set; }

        /// <summary>允收稽核時間</summary>
        [Column("Pre_Date", TypeName = "smalldatetime")]
        [Display(Name = "允收稽核時間")]
        public DateTime? Pre_Date { get; set; }

        /// <summary>處理稽核時間</summary>
        [Column("Tre_Pre_Date", TypeName = "smalldatetime")]
        [Display(Name = "處理稽核時間")]
        public DateTime? Tre_Pre_Date { get; set; }

        /// <summary>預定數量</summary>
        [Column("Pre_Qty")]
        [Display(Name = "預定數量")]
        public int? Pre_Qty { get; set; }

        /// <summary>稽核進度(1:填寫中,S:提出申請,M:待補正,Y1:通過)</summary>	
        [Column("Aud_State")]
        [Display(Name = "稽核進度(1:填寫中,S:提出申請,M:待補正,Y1:通過)")]
        public string Aud_State { get; set; }

        /// <summary>審核進度(1:填寫中,S:提出申請,M:待補正,Y1:通過)</summary>	
        [Column("Check_State")]
        [Display(Name = "審核進度(1:填寫中,S:提出申請,M:待補正,Y1:通過)")]
        public string Check_State { get; set; }

        [Column("Aud_Su_Date")]
        [Display(Name = "送出審核時間")]
        public DateTime? Aud_Su_Date { get; set; }

        /// <summary>申請人id</summary>	
        [Column("Uid")]
        [Display(Name = "申請人id")]
        [NeverUpdate]
        public int Uid { get; set; }

        /// <summary>公司名稱</summary>	
        [Column("CompanyName")]
        [Display(Name = "公司名稱")]
        [NeverUpdate]
        public string CompanyName { get; set; }
        [Column("Aud_Su_opinion")]
        [Display(Name = "投入產出表說明")]
        [MaxLength(250,ErrorMessage = "{0} 不可以超過250個字")]
        public string Aud_Su_opinion { get; set; }
        [Column("Aud_Desc")]
        [Display(Name = "審查意見")]
        [MaxLength(250, ErrorMessage = "{0} 不可以超過250個字")]
        public string Aud_Desc { get; set; }

        [NotMapped]
        public List<string> Bno { get; set; }
    }

    [Table("ScheduleAudit_SB", Schema = "Apply")]
    public partial class ScheduleAudit_SB
    {
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

        /// <summary>申請編號(民國年+月+日+3碼流水號)</summary>	
        [Column("bookingno")]
        [Display(Name = "申請編號(民國年+月+日+3碼流水號)")]
        [StringLength(10)]
        [NeverUpdate]
        public string Bookingno { get; set; }

        /// <summary>申請日期</summary>	
        [Column("App_Date", TypeName = "datetime")]
        [Display(Name = "申請日期")]
        [NeverUpdate]
        public DateTime App_Date { get; set; }
        /// <summary>申請人id</summary>	
        [Column("Uid")]
        [Display(Name = "申請人id")]
        [NeverUpdate]
        public int Uid { get; set; }
        [Column("Compare_State")]
        [Display(Name = "是否比對完成")]
        public bool? Compare_State { get; set; }
    }
}
