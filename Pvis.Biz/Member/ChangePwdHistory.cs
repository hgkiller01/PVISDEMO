using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Pvis.Biz.Member
{
    /// <summary>
    /// 密碼變更紀錄資訊
    /// </summary>
    [Table("ChangePwdHistory", Schema = "dbo")]
    public class ChangePwdHistory
    {

        /// <summary>
        /// 系統流水號
        /// </summary>
        [Key]
        public int Pid { get; set; }

        /// <summary>
        /// 對應 User 基本資料中的 Id 識別號碼
        /// </summary>
        [Required]
        [StringLength(450)]
        public string Id { get; set; }

        /// <summary>
        /// 雜湊密碼
        /// </summary>
        [Required]
        [StringLength(255)]
        public string PasswordHash { get; set; }

        /// <summary>
        /// 異動日期
        /// </summary>
        [Required]
        public DateTime LogDt { get; set; }
    }
}
