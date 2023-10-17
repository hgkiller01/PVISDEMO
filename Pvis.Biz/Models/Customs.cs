using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace Pvis.Biz.Models
{
    /// <summary>海關進口資料</summary>
    [Table("Customs", Schema = "profile")]
    public partial class Customs
    {
        /// <summary>項次</summary>
        [Column("ID")]
        public int ID { get; set; }
      
        /// <summary>業者統編</summary>
        [Column("Fac_ID")]
        public string Fac_ID { get; set; }

        /// <summary>業者名稱</summary>
        [Column("Fac_Name")]
        public string Fac_Name { get; set; }

        /// <summary>報關日期</summary>
        [Column("CustDate")]
        public DateTime CustDate { get; set; }

        /// <summary>報單號碼</summary>
        [Column("CustNo")]
        public string CustNo { get; set; }

        /// <summary>貨品分類號列</summary>
        [Column("ProdTypeCode")]
        public string ProdTypeCode { get; set; }

        /// <summary>統計用數量</summary>
        [Column("Qty")]
        public int Qty { get; set; }

        /// <summary>統計用單位</summary>
        [Column("Unit")]
        public string Unit { get; set; }

        /// <summary>淨重(公斤)</summary>	
        [Column("Kilogram", TypeName = "decimal(18, 2)")]
        public decimal Kilogram { get; set; }

        /// <summary>貨物名稱</summary>
        [Column("ProdName")]
        public string ProdName { get; set; }
    }
}
