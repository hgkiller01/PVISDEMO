using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Pvis.Biz.CustomizedAttr;

namespace Pvis.Biz.Models
{
    /// <summary>案場排出登記表</summary>
    [Table("Scrap_booking", Schema = "apply")]
    public partial class ScrapBooking
    {
        /// <summary>申請id</summary>	
        [Key]
        [Column("pid")]
        [Display(Name = "申請id")]
        public int Pid { get; set; }

        /// <summary>申請編號(民國年+月+日+3碼流水號)</summary>	
        [Required]
        [Column("bookingno")]
        [Display(Name = "申請編號(民國年+月+日+3碼流水號)")]
        [StringLength(10)]
        [NeverUpdate]
        public string Bookingno { get; set; }

        /// <summary>申請人id</summary>	
        [Column("uid")]
        [Display(Name = "申請人id")]
        [NeverUpdate]
        public int Uid { get; set; }

        /// <summary>申請日期</summary>	
        [Column("appdate", TypeName = "smalldatetime")]
        [Display(Name = "申請日期")]
        [NeverUpdate]
        public DateTime Appdate { get; set; }

        /// <summary>聯絡人*</summary>	
        [Required(ErrorMessage = "聯絡人為必填")]
        [Column("contact")]
        [Display(Name = "聯絡人*")]
        [StringLength(50)]
        public string Contact { get; set; }

        /// <summary>聯絡電話*</summary>	
        [Phone(ErrorMessage = "電話格式錯誤")]
        [Required(ErrorMessage = "聯絡電話為必填")]
        [Column("tel")]
        [Display(Name = "聯絡電話*")]
        [StringLength(20)]
        public string Tel { get; set; }

        /// <summary>E-mail*</summary>	
        [Required(ErrorMessage = "Email為必填")]
        [EmailAddress(ErrorMessage = "Email格式錯誤")]
        [Column("email")]
        [Display(Name = "E-mail*")]
        [StringLength(50)]
        public string Email { get; set; }

        /// <summary>存放地點id(存放地點資料維護)</summary>	
        [Column("uspid")]
        [Required(ErrorMessage = "存放地點為必填")]
        [Display(Name = "存放地點id(存放地點資料維護)")]
        public int Uspid { get; set; }

        /// <summary>Y1:通過(派車清運),Y2:通過(自行清運),N:未通過,M:補正,S:提出申請</summary>	
        [Column("status")]
        [Display(Name = "Y1:通過(派車清運),Y2:通過(自行清運),N:未通過,M:補正,S:提出申請")]
        [StringLength(2)]
        public string Status { get; set; }

        /// <summary>審核日期</summary>	
        [Column("ckdate", TypeName = "smalldatetime")]
        [Display(Name = "審核日期")]
        public DateTime? Ckdate { get; set; }

        /// <summary>審核人員</summary>	
        [Column("ckuid")]
        [Display(Name = "審核人員")]
        public int? Ckuid { get; set; }

        /// <summary>審核意見</summary>	
        [Column("opinion")]
        [StringLength(350)]
        [Display(Name = "審核意見")]
        public string Opinion { get; set; }

        /// <summary>Y1,指定清運時間</summary>	
        [Column("cleandate", TypeName = "smalldatetime")]
        [Display(Name = "Y1,指定清運時間")]
        public DateTime? Cleandate { get; set; }

        /// <summary>Y2,自行清運地點</summary>	
        [Column("ckspid")]
        [Display(Name = "Y2,自行清運地點")]
        public int? Ckspid { get; set; }


        /// <summary>Y:已安排日期,D:已完成清理</summary>	
        [Column("isSch")]
        [Display(Name = "Y:已安排日期,D:已完成清理")]
        public string isSch { get; set; }

        [Column("contract")]
        [Display(Name = "是否是拆除工程相關合約")]
        public bool? Contract { get; set; }

        [NotMapped]
        [Display(Name ="處理機構")]
        public string Tre_Name { get; set; }

        [NotMapped]
        [Column("qty")]
        public int? Qty { get; set; }

        [NotMapped]
        public string Pvid { get; set; }

        [NotMapped]
        public List<UserSpInfo> SP { get; set; }

        [NotMapped]
        public List<UserPvInfo> PV { get; set; }

        [NotMapped]
        public List<UserStoreAddress> USAddr { get; set; }

        [NotMapped]
        public string SBCity { get; set; }

        [NotMapped]
        public string USAddr1 { get; set; }

        [NotMapped]
        public decimal? SPWeight1 { get; set; }

        [NotMapped]
        public int? Al_frameYQty { get; set; }

        [NotMapped]
        public int? Al_frameNQty { get; set; }
        [NotMapped]
        public string CompanyName { get; set; }
        [NotMapped]
        public DateTime? Enter_Date { get; set; }
        [NotMapped]
        public string UserName { get; set; }

    }
}
