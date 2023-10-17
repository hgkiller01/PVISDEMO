using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Pvis.Biz.Models
{
    [Table("ProofSingle", Schema = "apply")]
    public class ProofSingle
    {
        [Key]
        public int PsId { get; set; }
        [Display(Name = "證明單編號")]
        [ForeignKey("ProofNo")]
        public string ProofNo { get; set; }
        [Display(Name = "核發月份")]
        [Required(ErrorMessage ="{0}必填")]
        public int Month { get; set; }
        [Display(Name = "核發年份")]
        public int? Year { get; set; }
        [Display(Name = "案場排出允收稽核說明")]
        [Required(ErrorMessage ="{0}必填")]
        [MaxLength(250,ErrorMessage ="{0}超過250個字")]
        public string PreDesc { get; set; }
        [Display(Name = "清除稽核說明")]
        [Required(ErrorMessage ="{0}必填")]
        [MaxLength(250, ErrorMessage = "{0}超過250個字")]
        public string CleDesc { get; set; }
        [Display(Name = "處理/境外處理/貯存稽核說明")]
        [Required(ErrorMessage = "{0}必填")]
        [MaxLength(250, ErrorMessage = "{0}超過250個字")]
        public string OtherDesc { get; set; }
        [Display(Name = "証明單是否己產出")]
        [Required]
        public bool IsOutput { get; set; }
        [Display(Name = "証明單產出日期")]
        public DateTime? OutPutDate { get; set; }
        public int Uid { get; set; }
        public DateTime CreateDate { get; set; }
        public int CreateUserId { get; set; }
        [Display(Name = "稽核行程編號")]
        [Required(ErrorMessage = "{0}必填")]
        public string Aud_Sch_No { get; set; }
        [NotMapped]
        public string UrlContent { get; set; }
        [NotMapped]
        public string OriginalFileName { get; set; }
    }
}
