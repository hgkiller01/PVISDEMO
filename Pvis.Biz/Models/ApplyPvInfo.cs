using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace Pvis.Biz.Models
{
    /// <summary>審查通過-設備登記資料</summary>
    [Table("ApplyPvInfo", Schema = "profile")]
    public partial class ApplyPvInfo
    {
        /// <summary>案場排出登記表Pid</summary>
        public int SBPid { get; set; }
        /// <summary>原設備登記Pid</summary>
        [Column("pid")]
        public int Pid { get; set; }
        /// <summary>設備登記編號</summary>
        [Column("pvno")]
        public string Pvno { get; set; }
        /// <summary>型態(地址1,地號2)</summary>
        [Column("addr_type")]
        public string AddrType { get; set; }
        /// <summary>設置地址</summary>
        [Column("pvaddr")]
        public string Pvaddr { get; set; }
        /// <summary>併網日期</summary>
        [Column("startdate", TypeName = "date")]
        public DateTime Startdate { get; set; }
        /// <summary>單一設備裝置容量(瓩)</summary>
        [Column("kilowatt", TypeName = "decimal(18, 3)")]
        public decimal Kilowatt { get; set; }
        /// <summary>總裝置容量(瓩)</summary>
        [Column(TypeName = "decimal(18, 3)")]
        public decimal Allkilowatt { get; set; }
        /// <summary>設備數量(片)</summary>
        public int SpQty { get; set; }
        /// <summary>備案編號</summary>
        public string Bno { get; set; }

        /// <summary>已確認申請量(片)</summary>
        [NotMapped]
        public int ReviewQty { get; set; }
        /// <summary>太陽能板資料</summary>
        [NotMapped]
        public List<UserSpInfo> SP { get; set; }
        /// <summary>設備登記申請表</summary>
        [NotMapped]
        public List<FormFileUpload> FileApplyDoc { get; set; }
        /// <summary>裝置容量證明文件</summary>
        [NotMapped]
        public List<FormFileUpload> FileProvDoc { get; set; }
        /// <summary>設備登記同意函</summary>
        [NotMapped]
        public List<FormFileUpload> FileAgreement { get; set; }
    }
}
