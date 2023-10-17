using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Pvis.Biz.Models
{
    [Table("PvImport", Schema = "apply")]
    public class PvImport
    {
        [Key]
        public int Id { get; set; }

        [Display(Name = "稽核行程編號")]
        [Required(ErrorMessage = "無稽核行程編號")]
        public string Aud_Sch_No { get; set; }

        [Display(Name = "案場排出登記編號")]
        [Required(ErrorMessage = "無案廠排出登記編號")]
        public string BookingNoId { get; set; }

        [Display(Name = "有序號")]
        [Required(ErrorMessage = "{0}數量未填寫，如無請填「0」")]
        public int AL_O_SN_O { get; set; }

        [Display(Name = "無序號")]
        [Required(ErrorMessage = "{0}數量未填寫")]
        public int AL_O_SN_X { get; set; }

        [Display(Name = "序號不符")]
        [Required(ErrorMessage = "{0}數量未填寫")]
        public int AL_O_SN_N { get; set; }

        [Display(Name = "有序號")]
        [Required(ErrorMessage = "{0}數量未填寫")]
        public int AL_X_SN_O { get; set; }

        [Display(Name = "無序號")]
        [Required(ErrorMessage = "{0}數量未填寫")]
        public int AL_X_SN_X { get; set; }

        [Display(Name = "序號不符")]
        [Required(ErrorMessage = "{0}數量未填寫")]
        public int AL_X_SN_N { get; set; }

        [Display(Name = "PV重量(公斤)")]
        [Required(ErrorMessage = "{0}數量未填寫")]
        public double PV_Weight { get; set; }

        [Display(Name = "包材重(公斤)")]
        [Required(ErrorMessage = "{0}數量未填寫")]
        public double Pkg_Weight { get; set; }

        public string CompanyName { get; set; }

        public string Creator { get; set; }

        public DateTime CreateDate { get; set; }

        public string Modifier { get; set; }

        public DateTime ModifyDate { get; set; }

        public byte State { get; set; }
    }
}
