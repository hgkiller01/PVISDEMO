using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace Pvis.Biz.Models
{
    /// <summary>模組製造業名單</summary>
    [Table("ModuleFacList", Schema = "profile")]
    public partial class ModuleFacList
    {

        /// <summary>項次</summary>
        [Column("ID")]
        public int ID { get; set; }

        /// <summary>事業名稱</summary>
        [Column("Fac_Name")]
        public string Fac_Name { get; set; }

        /// <summary>機構電話</summary>
        [Column("Fac_Tel")]
        public string Fac_Tel { get; set; }

        /// <summary>機構地址</summary>
        [Column("Company_Addr")]
        public string Company_Addr { get; set; }

        /// <summary>工廠地址</summary>
        [Column("Fac_Addr")]
        public string Fac_Addr { get; set; }
    }
}
