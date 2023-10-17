using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Pvis.Biz.Models
{
    /// <summary>案場排出登記表-審查確認</summary>
    [Table("ScrapBookingReview", Schema = "apply")]
    public partial class ScrapBookingReview
    {
        /// <summary>流水號</summary>
        [Key]
        public int Pid { get; set; }

        /// <summary>案場排出登記表Pid</summary>
        public int SBPid { get; set; }

        /// <summary>Y1:通過(派車清運),Y2:通過(自行清運),N:未通過,M:補正,S:提出申請</summary>
        [Required(ErrorMessage = "『確認狀態』未填寫")]
        [StringLength(2)]
        public string Status { get; set; }

        /// <summary>確認日期</summary>
        [Column(TypeName = "datetime")]
        public DateTime CKDate { get; set; }

        /// <summary>確認者</summary>
        public int CKUid { get; set; }

        /// <summary>確認者</summary>
        [StringLength(100)]
        public string CKName { get; set; }

        /// <summary>清運日期</summary>
        [Column(TypeName = "datetime")]
        public DateTime? CLDate { get; set; }

        /// <summary>清運地點Pid</summary>
        public int CLPid { get; set; }

        /// <summary>意見</summary>
        [StringLength(50)]
        public string Desc { get; set; }

        [NotMapped]
        public ClearLocation CL {get; set;}
    }
}
