using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Pvis.Biz.CustomizedAttr;

namespace Pvis.Biz.Models
{
    /// <summary>設備登記資料</summary>
    [Table("user_pv_info", Schema = "profile")]
    public partial class UserPvInfo
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

        /// <summary>設備登記編號*</summary>	
        [Required(ErrorMessage = "設備登記編號為必填")]
        [Column("pvno")]
        [StringLength(50)]
        [Display(Name = "設備登記編號*")]
        [NeverUpdate]
        public string Pvno { get; set; }
        /// <summary>型態(地址1,地號2)</summary>
        [Required]
        [Display(Name = "型態(地址1,地號2)")]
        [Column("addr_type")]
        [StringLength(1)]
        public string AddrType { get; set; }

	
        /// <summary>設置地址</summary>	
        [Display(Name = "設置地址")]
        [Required(ErrorMessage = "設置地址為必填")]
        [Column("pvaddr")]
        [StringLength(100)]
        public string Pvaddr { get; set; }

        /// <summary>併網日期*</summary>	
        [Required(ErrorMessage = "併網日期為必填")]
        [Display(Name = "併網日期*")]
        [Column("startdate", TypeName = "date")]
        public DateTime Startdate { get; set; }

        /// <summary>單一設備裝置容量(瓩)*</summary>	
        [Display(Name = "單一設備裝置容量(瓩)*")]
        //[RegularExpression("^[0-9.]{0,}$", ErrorMessage = "單一設備裝置容量(瓩)需為數值")]
        [Range(0.001, 1000000, ErrorMessage = "單一設備裝置容量(瓩)需大於0")]
        [Column("kilowatt", TypeName = "decimal(18, 3)")]
        public decimal Kilowatt { get; set; }

        /// <summary>建立日期</summary>
        [Display(Name = "建立日期")]
        [Column("createdate", TypeName = "smalldatetime")]
        public DateTime Createdate { get; set; }

        /// <summary>狀態(啟用1,不啟用0)</summary>	
        [Required]
        [Display(Name = "狀態(啟用1,不啟用0)")]
        [Column("status")]
        [StringLength(1)]
        public string Status { get; set; }

        /// <summary>
        /// 總裝置容量(瓩)
        /// </summary>
        //[RegularExpression("^[0-9.]{0,}$", ErrorMessage = "總裝置容量(瓩)需為數值")]
        [Range(0.001, 1000000, ErrorMessage = "總裝置容量(瓩)需大於0")]
        [Column(TypeName = "decimal(18, 3)")]
        public decimal Allkilowatt { get; set; }


        /// <summary>
        /// 設備數量(片)
        /// </summary>
        [Display(Name = "設備數量(片)")]
        //[RegularExpression("^[1-9]{1}[0-9]{0,}$$", ErrorMessage = "設備數量需為整數")]
        [Range(1, 1000000, ErrorMessage = "設備數量(片)需大於0")]
        public int SpQty { get; set; }

        /// <summary>
        /// 備案編號
        /// </summary>
        [Required(ErrorMessage = "備案編號為必填")]
        [StringLength(50)]
        public string Bno { get; set; }

        /// <summary>再生能源發電設備型別及使用能源編號</summary>
        [Required(ErrorMessage = "再生能源發電設備型別及使用能源必填")]
        public byte PETypeID { get; set; }

        /// <summary>已確認申請量(片)</summary>
        [NotMapped]
        public int ReviewQty { get; set; }
        /// <summary>太陽能板資料</summary>
        [NotMapped]
        public List<UserSpInfo> SP { get; set; }
        /// <summary>設備登記申請表</summary>
        [NotMapped]
        public List<FormFileUpload> FileApplyDoc { get; set; }
        /// <summary>裝置容量證明文件</summary>
        [NotMapped]
        public List<FormFileUpload> FileProvDoc { get; set; }
        /// <summary>設備登記同意函</summary>
        [NotMapped]
        public List<FormFileUpload> FileAgreement { get; set; }
        /// <summary>
        /// 設備登記同意函
        /// </summary>
        [NotMapped]
        public List<FormFileUpload> FilePvSnDoc { get; set; }
        /// <summary>
        /// 設備已建之光電版數
        /// </summary>
        [NotMapped]
        public int TotalPvCount { get; set; }
    }
}
