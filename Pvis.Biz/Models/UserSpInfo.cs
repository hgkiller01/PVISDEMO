using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Pvis.Biz.CustomizedAttr;

namespace Pvis.Biz.Models
{
    /// <summary>太陽能板資料</summary>
    [Table("user_sp_info", Schema = "profile")]
    public partial class UserSpInfo
    {
        /// <summary>序號</summary>	
        [Key]
        [Column("pid")]
        [Display(Name = "序號")]
        public int Pid { get; set; }

        /// <summary>申請人id</summary>	
        [Column("uid")]
        [Display(Name = "申請人id")]
        [NeverUpdate]
        public int Uid { get; set; }

        /// <summary>設備登記ID</summary>	
        [Column("pvid")]
        [Display(Name = "設備登記ID")]
        public int? Pvid { get; set; }

        /// <summary>有無太陽光電板序號(有1,無0)*</summary>	
        [Required(ErrorMessage = "有無太陽光電板序號為必填")]
        [Column("hasno")]
        [Display(Name = "有無太陽光電板序號*")]
        [StringLength(1)]
        public string Hasno { get; set; }

        /// <summary>太陽光電板序號</summary>
        [Column("sno")]
        [Required(ErrorMessage = "太陽光電板序號為必填,若無則自訂")]
        [Display(Name = "太陽光電板序號")]
        [StringLength(50)]
        public string Sno { get; set; }

        /// <summary>廠牌*</summary>	
        [Required(ErrorMessage = "廠牌為必填")]
        [Display(Name = "廠牌*")]
        [StringLength(50)]
        public string Brand { get; set; }

        /// <summary>型號*</summary>	
        [Required(ErrorMessage = "型號為必填")]
        [Display(Name = "型號*")]
        [StringLength(50)]
        public string Module { get; set; }

        /// <summary>樣態*(1.矽晶單片玻璃 2.矽晶雙片玻璃 3.薄膜型)</summary>
        [Required(ErrorMessage = "樣態為必填")]
        [Display(Name = "樣態*")]
        [StringLength(1)]
        public string Style { get; set; }

        /// <summary>外觀鋁框完整度(有1,無0)*</summary>	
        [Required(ErrorMessage = "外觀鋁框完整度為必填")]
        [Column("Al_frame")]
        [Display(Name = "外觀鋁框完整度*")]
        [StringLength(1)]
        public string AlFrame { get; set; }

        /// <summary>備註無序號者，須備註說明</summary>
        [Column("memo")]
        [Display(Name = "備註無序號者，須備註說明")]
        [StringLength(100)]
        public string Memo { get; set; }

        /// <summary>建立日期</summary>
        [Column("createdate", TypeName = "smalldatetime")]
        [Display(Name = "建立日期")]
        public DateTime Createdate { get; set; }

        /// <summary>狀態(啟用1,不啟用0)</summary>	
        [Required]
        [Column("status")]
        [Display(Name = "狀態(啟用1,不啟用0)")]
        [StringLength(1)]
        public string Status { get; set; }

        /// <summary>案場排出登記ID</summary>
        [Column("SBid")]
        [Display(Name = "案場排出登記ID")]
        public int? Sbid { get; set; }

        /// <summary>出貨單號</summary>	
        [Display(Name = "出貨單號")]
        [StringLength(50)]
        public string Shipno { get; set; }

        /// <summary>樣態說明</summary>
        [StringLength(10)]
        public string StyleDesc { get; set; }

        /// <summary>重量</summary>
        [Range(0.1, 99, ErrorMessage = "太陽能板重量合理數值應為0.1~99KG")]
        [Column("SPWeight", TypeName = "decimal(5, 1)")]
        public decimal? Spweight { get; set; }

        [NotMapped]
        public string Pvno { get; set; }

        [NotMapped]
        public UserPvInfo PV { get; set; }

        /// <summary>附件</summary>
        [NotMapped]
        public List<FormFileUpload> File { get; set; }

        /// <summary>案場排出登記表-審查確認通過狀態</summary>
        [NotMapped]
        public bool ReviewStatus { get; set; }
    }
}
