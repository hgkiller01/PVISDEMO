using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Pvis.Biz.CustomizedAttr;

namespace Pvis.Biz.Models
{
    /// <summary>清理行程表</summary>
    [Table("Schedule", Schema = "Apply")]
    public partial class Schedule
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

        /// <summary>預定清理日期</summary>	
        [Column("Cle_Date", TypeName = "smalldatetime")]
        [Display(Name = "預定清理日期")]
        public DateTime Cle_Date { get; set; }

        ///// <summary>清理數量</summary>	
        //[Column("Cle_Qty")]
        //public int? Cle_Qty { get; set; }

        /// <summary>清除機構*</summary>	
        [Column("Cle_Name")]
        [Display(Name = "清除機構*")]
        [StringLength(50)]
        public string Cle_Name { get; set; }

        /// <summary>處理機構*</summary>	
        [Column("Tre_Name")]
        [Display(Name = "處理機構*")]
        [StringLength(50)]
        public string Tre_Name { get; set; }

        /// <summary>申請日期</summary>	
        [Column("App_Date", TypeName = "datetime")]
        [Display(Name = "申請日期")]
        [NeverUpdate]
        public DateTime App_Date { get; set; }

        /// <summary>清理進度(1:已安排日期,D:待確認合約,D1:已確認合約,DM:合約須補正,C1:已完成清理)</summary>	
        [Column("Cle_State")]
        [Display(Name = "清理進度(1:已安排日期,D:待確認合約,D1:已確認合約,DM:合約須補正,C1:已完成清理)")]
        public string Cle_State { get; set; }

        /// <summary>申請人id</summary>	
        [Column("Uid")]
        [Display(Name = "申請人id")]
        [NeverUpdate]
        public int Uid { get; set; }

        /// <summary>進廠時間</summary>
        [Column("Enter_Date", TypeName = "smalldatetime")]
        [Display(Name = "進廠時間")]
        public DateTime? Enter_Date { get; set; }

        /// <summary>進廠重量(含廢PV過磅) (公斤)</summary>	
        [Column("Full_Weight", TypeName = "decimal(18,3)")]
        [Display(Name = "進廠重量(含廢PV過磅) (公斤)")]
        public decimal? Full_Weight { get; set; }

        /// <summary>進廠空車重(公斤)</summary>	
        [Column("EmptyCar_Weight", TypeName = "decimal(18,3)")]
        [Display(Name = "進廠空車重(公斤)")]
        public decimal? EmptyCar_Weight { get; set; }

        ///// <summary>有鋁框數量(片)</summary>	
        [Column("AlFrameY_Qty")]
        public int? AlFrameY_Qty { get; set; }

        ///// <summary>無鋁框數量(片)</summary>	
        [Column("AlFrameN_Qty")]
        public int? AlFrameN_Qty { get; set; }

        /// <summary>清除機構文件</summary>
        [NotMapped]
        public List<FormFileUpload> FileCleDoc { get; set; }

        /// <summary>處理機構文件</summary>
        [NotMapped]
        public List<FormFileUpload> FileTreDoc { get; set; }

        [NotMapped]
        public List<string> Bno { get; set; }

        [NotMapped]
        public int? Sum_Qty { get; set; }

        [NotMapped]
        public decimal? Sum_Weight { get; set; }
    }

    [Table("Schedule_SB", Schema = "Apply")]
    public partial class Schedule_SB 
    {
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
    }

}
