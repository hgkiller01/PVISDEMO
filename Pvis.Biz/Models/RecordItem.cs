using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Pvis.Biz.Models
{
    [Table("RecordItem", Schema = "apply")]
    public class RecordItem
    {
        [Key]
        public int RecordIemID { get; set; }
        [Display(Name = "類別")]
        [Required]
        public string Code_Type { get; set; }
        [Display(Name = "代碼")]
        [Required(ErrorMessage = "{0} 必填")]
        public string Code_no { get; set; }
        [Display(Name = "名稱")]
        [Required(ErrorMessage = "{0} 必填")]
        public string Code_name { get; set; }
        [Display(Name ="其他物料名稱")]
        public string ItemName { get; set; }
        [Display(Name ="重量(kg)")]
        [Required(ErrorMessage = "{0} 必填")]
        public double? ItemWeight { get; set; }
        [Display(Name ="處理程序")]
        [Required(ErrorMessage = "{0} 必選")]
        public int Process { get; set; }
        [Display(Name ="產出日期")]
        [Required(ErrorMessage ="{0} 必填")]
        public DateTime? MakeDate { get; set; }
        [Display(Name = "稽核行程編號")]
        [Required]
        public string Aud_Sch_No { get; set; }
        [Display(Name ="處理廠/貯存場機構名稱")]
        public string CompanyName { get; set; }
        public DateTime CreateDate { get; set; }
        public int CreateUserID { get; set; }
        public string CreateUserName { get; set; }
        public DateTime? ModifyDate { get; set; }
        public int? ModifyUserID { get; set; }
        public string ModifyUserName { get; set; }
    }
}
