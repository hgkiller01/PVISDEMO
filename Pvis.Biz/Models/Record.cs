using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Pvis.Biz.Models
{
    [Table("Record", Schema = "apply")]
    public class Record
    {
        [Key]
        public int RecordID { get; set; }
        [ForeignKey("RecordItemID")]
        [Display(Name ="物料名稱")]
        [Required]
        public int RecordItemID { get; set; }
        [Display(Name = "稽核行程編號")]
        [Required]
        public string Aud_Sch_No { get; set; }
        [Display(Name ="廢棄物聯單編號")]
        public string WasteSn { get; set; }
        [Display(Name ="廠商名稱")]
        [Required(ErrorMessage = "{0} 必填")]
        public string VendorName { get; set; }
        [Display(Name = "用途")]
        [Required(ErrorMessage = "{0} 必填")]
        public string UseFor { get; set; }
        [Display(Name = "許可字號")]
        public string ExportNumber { get; set; }
        [Display(Name ="出廠時間")]
        [Required(ErrorMessage = "{0} 必填")]
        public DateTime? ShipmentDate { get; set; }
        [Display(Name ="重量(kg)")]
        [Required(ErrorMessage ="{0} 必填")]
        public double? RecordWeight { get; set; }
        [Display(Name ="輸出國別")]
        public string CountryName { get; set; }
        [Display(Name ="貨櫃號碼")]
        public string ContainerSn { get; set; }
        public string CompanyName { get; set; }
        [Display(Name ="日期")]
        public DateTime CreateDate { get; set; }
        public int CreateUserID { get; set; }
        public string CreateUserName { get; set; }
        public DateTime? ModifyDate { get; set; }
        public int? ModifyUserID { get; set; }
        public string ModifyUserName { get; set; }
    }
}
