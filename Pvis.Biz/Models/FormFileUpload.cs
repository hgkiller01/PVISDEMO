using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Pvis.Biz.CommEnum;

namespace Pvis.Biz.Models
{
    [Table("FormFileUpload", Schema = "msg")]
    public partial class FormFileUpload
    {
        public int Id { get; set; }
        [Required]
        [StringLength(50)]
        public string AppId { get; set; }
        public eDocType DocType { get; set; }
        public eItemType ItemType { get; set; }
        [Required]
        [StringLength(500)]
        public string FilePath { get; set; }
        [Required]
        [StringLength(20)]
        public string FileExtName { get; set; }
        public double FileSize { get; set; }
        [Required]
        [StringLength(500)]
        public string OriginalFileName { get; set; }
        [Required]
        [StringLength(500)]
        public string Memo { get; set; }
        public int CreateUid { get; set; }
        [Column(TypeName = "datetime")]
        public DateTime CreateDt { get; set; }
        public int ModUid { get; set; }
        [Column(TypeName = "datetime")]
        public DateTime ModDt { get; set; }
    }
}
