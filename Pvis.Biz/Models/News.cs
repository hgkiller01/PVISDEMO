using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Pvis.Biz.Models
{
    [Table("News", Schema = "msg")]
    public partial class News
    {
        /// <summary>流水號</summary>
        [Key]
        public int Pid { get; set; }

        /// <summary>主旨</summary>
        [Required]
        [StringLength(250)]
        [Display(Name = "主旨")]
        public string Subject { get; set; }

        /// <summary>訊息內容</summary>
        [Required]
        [Display(Name = "訊息內容")]
        public string Body { get; set; }

        /// <summary>上下架</summary>
        public bool IsEnable { get; set; }

        /// <summary>建立日期</summary>
        [Column(TypeName = "datetime")]
        public DateTime CreateDt { get; set; }

        /// <summary>建立者</summary>
        public int CreateUid { get; set; }

        /// <summary>異動日期</summary>
        [Column(TypeName = "datetime")]
        public DateTime ModDt { get; set; }

        /// <summary>異動者</summary>
        public int ModUid { get; set; }

        /// <summary>發布日期</summary>
        [Display(Name = "發布日期")]
        [Required]
        [Column(TypeName = "datetime")]
        public DateTime? PostDt { get; set; }

        /// <summary>下架日期</summary>
        [Display(Name = "下架日期")]
        [Column(TypeName = "datetime")]
        public DateTime? ExpireDt { get; set; }
    }
}
