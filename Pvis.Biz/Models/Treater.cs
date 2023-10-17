using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Pvis.Biz.Models
{
    [Table("Treater", Schema = "msg")]
    public partial class Treater
    {
        /// <summary>流水號</summary>
        [Key]
        public int Pid { get; set; }

        /// <summary>縣市別</summary>
        public string City { get; set; }

        /// <summary>機構名稱</summary>
        public string Company { get; set; }

        /// <summary>級別</summary>
        public string Grade { get; set; }

        /// <summary>機構電話</summary>
        public string Tel { get; set; }
    }
}
