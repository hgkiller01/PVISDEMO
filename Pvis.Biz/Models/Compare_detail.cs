using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Pvis.Biz.Models
{
    [Table("Compare_detail", Schema = "Apply")]
    public class Compare_detail
    {
        [Key]
        [Column("CDID")]
        [Display(Name = "流水號")]
        public int CDID { get; set; }
        [Column("Spid")]
        [Display(Name ="太陽能光電版流水號")]
        public int? Spid { get; set; }
        /// <summary>
        /// 案場編號
        /// </summary>
        [Column("bookingno")]
        public string bookingno { get; set; }
        /// <summary>
        /// 稽核編號
        /// </summary>
        [Column("Aud_Sch_No")]
        public string Aud_Sch_No { get; set; }
        [Column("Compare_Sno")]
        [Display(Name = "太陽能板比對條碼")]
        public string Compare_Sno { get; set; }
        /// <summary>
        /// 外觀鋁框
        /// </summary>
        [Column("Al_frame")]
        [Display(Name = "外觀鋁框(有1,無0)")]
        public string Al_frame { get; set; }
        /// <summary>
        /// 比對狀態
        /// </summary>
        [Column("status")]
        [Display(Name = "比對狀態")]
        public bool? status { get; set; }
        /// <summary>
        /// 比對時間
        /// </summary>
        [Column("CompareTime")]
        [Display(Name = "比對時間")]
        public DateTime CompareTime { get; set; }
        [Column("CreateUser")]
        public int CreateUser { get; set; }
        [NotMapped]
        public string Sno { get; set; }
    }
}
