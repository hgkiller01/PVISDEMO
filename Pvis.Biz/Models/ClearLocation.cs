using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Pvis.Biz.Models
{
    /// <summary>清運地點</summary>
    [Table("ClearLocation", Schema = "msg")]
    public partial class ClearLocation
    {
        /// <summary>流水號</summary>
        [Key]
        public int Pid { get; set; }

        /// <summary>名稱</summary>
        [Required(ErrorMessage = "『名稱』未填寫")]
        [StringLength(100)]
        public string Name { get; set; }

        /// <summary>地址</summary>
        [Required(ErrorMessage = "『地址』未填寫")]
        [StringLength(100)]
        public string Addr { get; set; }

        /// <summary>聯絡電話</summary>
        [Required(ErrorMessage = "『聯絡電話』未填寫")]
        [Phone(ErrorMessage = "『聯絡電話』格式錯誤")]
        [StringLength(20)]
        public string Tel { get; set; }

        /// <summary>狀態</summary>
        [Required(ErrorMessage = "『狀態』未填寫")]
        public bool? Status { get; set; }

        /// <summary>建立日期</summary>
        [Column(TypeName = "datetime")]
        public DateTime InsDT { get; set; }

        /// <summary>建立者</summary>
        public int InsUid { get; set; }

        /// <summary>異動日期</summary>
        [Column(TypeName = "datetime")]
        public DateTime UpdDT { get; set; }

        /// <summary>異動者</summary>
        public int UpdUid { get; set; }
    }
}
