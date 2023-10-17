using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Pvis.Biz.Models
{
    [Table("PvTreatment", Schema = "apply")]
    public class PvTreatment
    {
        [Key]
        public int Id { get; set; }

        public int Pv_Imp_Id { get; set; }

        [Display(Name = "案廠排出登記申請編號")]
        public string Aud_Sch_No { get; set; }

        [Display(Name = "處理日期")]
        [Required(ErrorMessage = "{0}未填寫")]
        public DateTime TreatmentDate { get; set; }

        [Display(Name = "有序號")]
        [Required(ErrorMessage = "有鋁框{0}數量未填寫")]
        public int AL_O_SN_O { get; set; }

        [Display(Name = "無序號")]
        [Required(ErrorMessage = "有鋁框{0}數量未填寫")]
        public int AL_O_SN_X { get; set; }

        [Display(Name = "序號不符")]
        [Required(ErrorMessage = "有鋁框{0}數量未填寫")]
        public int AL_O_SN_N { get; set; }

        [Display(Name = "有序號")]
        [Required(ErrorMessage = "無鋁框{0}數量未填寫")]
        public int AL_X_SN_O { get; set; }

        [Display(Name = "無序號")]
        [Required(ErrorMessage = "無鋁框{0}數量未填寫")]
        public int AL_X_SN_X { get; set; }

        [Display(Name = "序號不符")]
        [Required(ErrorMessage = "無鋁框{0}數量未填寫")]
        public int AL_X_SN_N { get; set; }

        [Display(Name = "重量(公斤)")]
        [Required(ErrorMessage = "{0}數量未填寫")]
        public double Weight { get; set; }

        [Display(Name = "公司名稱")]
        public string CompanyName { get; set; }

        public string Creator { get; set; }

        public DateTime CreateDate { get; set; }

        public string Modifier { get; set; }

        public DateTime ModifyDate { get; set; }

        public byte State { get; set; }
    }
}
