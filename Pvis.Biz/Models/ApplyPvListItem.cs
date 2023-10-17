using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace Pvis.Biz.Models
{
    /// <summary>太陽光電設備登記核准案件清冊-太陽光電板序號 (能源局匯入)</summary>
    [Table("ApplyPvListItem", Schema = "profile")]
    public partial class ApplyPvListItem
    {
        /// <summary>項次</summary>
        [Column("ID")]
        public int ID { get; set; }

        /// <summary>設備登記編號</summary>
        [Column("PVNo")]
        public string PVNo { get; set; }

        /// <summary>出廠日期</summary>
        [Column("ExFacDate", TypeName = "date")]
        public DateTime? ExFacDate { get; set; }

        /// <summary>出貨單號</summary>	
        [Column("Shipno")]
        public string Shipno { get; set; }

        /// <summary>廠牌</summary>	
        [Column("Brand")]
        public string Brand { get; set; }

        /// <summary>型號</summary>	
        [Column("Module")]
        public string Module { get; set; }

        /// <summary>太陽光電板序號</summary>
        [Column("sno")]
        public string Sno { get; set; }

        /// <summary>單一設備裝置容量(瓩)*</summary>	
        [Column("kilowatt", TypeName = "decimal(18, 3)")]
        public decimal Kilowatt { get; set; }
    }
}
