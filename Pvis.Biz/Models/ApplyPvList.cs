using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace Pvis.Biz.Models
{
    /// <summary>太陽光電設備登記核准案件清冊(能源局匯入)</summary>
    [Table("ApplyPvList", Schema = "profile")]
    public partial class ApplyPvList
    {

        /// <summary>項次</summary>
        [Column("ID")]
        public int ID { get; set; }

        /// <summary>申請人類別</summary>
        [Column("ApplicantType")]
        public string ApplicantType { get; set; }

        /// <summary>申請人</summary>
        [Column("Applicant")]
        public string Applicant { get; set; }

        /// <summary>申請人ID</summary>
        [Column("ApplicantID")]
        public string ApplicantID { get; set; }

        /// <summary>申請人地址</summary>
        [Column("ApplicantAddr")]
        public string ApplicantAddr { get; set; }

        /// <summary>負責人</summary>
        [Column("Supervisor")]
        public string Supervisor { get; set; }

        /// <summary>單一設備裝置容量(瓩)</summary>
        [Column("Kilowatt")]
        public string Kilowatt { get; set; }

        /// <summary>設備數量(片)</summary>
        [Column("SpQty")]
        public string SpQty { get; set; }

        /// <summary>總裝置容量(瓩)</summary>
        [Column("AllKilowatt")]
        public string AllKilowatt { get; set; }

        /// <summary>設置場址(地址)</summary>
        [Column("PVAddr")]
        public string PVAddr { get; set; }
        /// <summary>設置場址(地號)</summary>
        [Column("PVCadastre")]
        public string PVCadastre { get; set; }

        /// <summary>同意備案編號</summary>
        [Column("BNo")]
        public string BNo { get; set; }

        /// <summary>同意備案核准日期</summary>
        [Column("BAppDate", TypeName = "date")]
        public DateTime? BAppDate { get; set; }

        /// <summary>完工併聯日期</summary>
        [Column("FinDate", TypeName = "date")]
        public DateTime FinDate { get; set; }

        /// <summary>設備登記編號</summary>
        [Column("PVNo")]
        public string PVNo { get; set; }

        /// <summary>設備登記核准日期</summary>
        [Column("PVAppDate", TypeName = "date")]
        public DateTime PVAppDate { get; set; }

        /// <summary>預計除役時間</summary>
        [NotMapped]
        public DateTime DecomDate {
            get { return FinDate.AddDays(7300); }
        }

        /// <summary>再生能源發電設備型別及使用能源編號</summary>
        [Column("PETypeID")]
        public byte PETypeID { get; set; }

        /// <summary>是否顯示詳細檢視</summary>
        [NotMapped]
        public bool isShow { get; set; }
        

    }
}
