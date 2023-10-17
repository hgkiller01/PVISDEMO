using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Pvis.Biz.CustomizedAttr;

namespace Pvis.Biz.Models
{
    [Table("user_store_address", Schema = "profile")]
    public partial class UserStoreAddress
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

        /// <summary>型態(地址1,地號2)</summary>	
        [Required]
        [Column("addr_type")]
        [Display(Name = "型態(地址1,地號2)")]
        [StringLength(1)]
        [NeverUpdate]
        public string AddrType { get; set; }

        /// <summary>存放地點</summary>	
        [Required(ErrorMessage = "存放地點為必填")]
        [Column("storeaddr")]
        [Display(Name = "存放地點")]
        [StringLength(100)]
        [NeverUpdate]
        public string Storeaddr { get; set; }

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
    }
}
