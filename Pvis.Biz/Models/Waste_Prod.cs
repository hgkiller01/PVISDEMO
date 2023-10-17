using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Pvis.Biz.Models
{
    [Table("Waste_Prod", Schema = "code")]
    public partial class Waste_Prod
    {
        /// <summary>代碼種類 Waste=廢棄物, Prod=產品原物料</summary>
        public string Code_Type { get; set; }
        /// <summary>代碼</summary>
        [Key]
        public string Code_no { get; set; }
        /// <summary>代碼名稱</summary>
        public string Code_name { get; set; }
        /// <summary>是否已停用</summary>
        public string is_delete { get; set; }

    }
}
