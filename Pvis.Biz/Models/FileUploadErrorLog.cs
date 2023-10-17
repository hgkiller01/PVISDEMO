using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;
using Pvis.Biz.CommEnum;

namespace Pvis.Biz.Models
{
    [Table("FileUploadErrorLog", Schema = "msg")]
    public class FileUploadErrorLog
    {
        /// <summary>
        /// 流水號
        /// </summary>
        [Key]
        public int LogID { get; set; }
        /// <summary>
        /// 使用者ID
        /// </summary>
        public int? Uid { get; set; }
        /// <summary>
        /// 檢附文件型態
        /// </summary>
        public eDocType DocType { get; set; }
        /// <summary>
        /// 文件項目種類
        /// </summary>
        public eItemType ItemType { get; set; }
        /// <summary>
        /// 原始檔案名稱
        /// </summary>
        public string OriginalFileName { get; set; }
        /// <summary>
        /// 上傳功能名稱
        /// </summary>
        public string UploadFunction { get; set; }
        /// <summary>
        /// IP位址
        /// </summary>
        public string IPAddress { get; set; }
        /// <summary>
        /// 錯誤原因
        /// </summary>
        public string ErrorMessage { get; set; }
        /// <summary>
        /// 錯誤時間
        /// </summary>
        public DateTime ErrorDate { get; set; }
    }
}
