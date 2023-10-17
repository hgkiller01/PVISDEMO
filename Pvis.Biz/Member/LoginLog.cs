using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Pvis.Biz.Member
{
    [Table("LoginLog", Schema = "dbo")]
    public class LoginLog
    {
        [Key]
        public int LogId { get; set; }
        /// <summary>
        /// 使用者帳號
        /// </summary>
        public string UserName { get; set; }
        /// <summary>
        /// 機構名稱
        /// </summary>
        public string CompanyName { get; set; }
        /// <summary>
        /// 顯示名稱
        /// </summary>
        public string DisplayName { get; set; }
        /// <summary>
        /// 使用者ID
        /// </summary>
        public int Uid { get; set; }
        /// <summary>
        /// 動作時間
        /// </summary>
        public DateTime ActionTime { get; set; }
        /// <summary>
        /// 動作類型:Login[登入] Logout[登出]
        /// </summary>
        public string ActionType { get; set; }
        /// <summary>
        /// 動作IP
        /// </summary>
        public string IP { get; set; }
    }
}
