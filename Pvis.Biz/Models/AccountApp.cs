using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Pvis.Biz.CommEnum;

namespace Pvis.Biz.Models
{
    [Table("AccountApp", Schema = "profile")]
    public partial class AccountApp
    {
        [Key]
        [Column("pid")]
        public int Pid { get; set; }
        [Required]
        [StringLength(250)]
        public string UserName { get; set; }
        [StringLength(250)]
        public string CompanyName { get; set; }
        [Required]
        [StringLength(250)]
        public string Address { get; set; }
        [StringLength(3)]
        [RegularExpression("^[A-Z][0-9]{2}$")]
        public string TownId { get; set; }
        [Required]
        [StringLength(120)]
        public string Tel { get; set; }
        [StringLength(120)]
        public string Email { get; set; }
        public bool? IsGetEuicNo { get; set; }
        [StringLength(10)]
        [RegularExpression("^[A-Z][0-9]{2}[A-Z0-9]{5}$",ErrorMessage = "管制編號格式錯誤")]
        public string EuicNo { get; set; }
        public AppRoleList UserRole { get; set; }
        public AppStatusList Status { get; set; }
        public string RejectReason { get; set; }
        [Column(TypeName = "datetime")]
        public DateTime CreateDt { get; set; }
        [Column(TypeName = "datetime")]
        public DateTime ModDt { get; set; }
        public int ModUid { get; set; }
        public int ReviewUid { get; set; }
        [Column(TypeName = "datetime")]
        public DateTime? ReviewDt { get; set; }
        [StringLength(8)]
        [RegularExpression("^[ABDEF][A-Z][0-9]{6}$")]
        public string ControlNo { get; set; }
        /// <summary>
        /// 非案場業者所有人(委託申請)
        /// </summary>
        public bool IsNotOwner { get; set; }
        [StringLength(250)]
        public string CaseName { get; set; }
        [StringLength(120)]
        public string CaseEmail { get; set; }
        public string IPAddress { get; set; }
    }
}
