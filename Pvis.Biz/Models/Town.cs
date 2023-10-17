using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Pvis.Biz.Models
{
    [Table("Town", Schema = "code")]
    public partial class Town
    {
        [Required]
        [StringLength(1)]
        public string CountyId { get; set; }
        [Required]
        [StringLength(5)]
        public string CountyName { get; set; }
        [StringLength(3)]
        public string TownId { get; set; }
        [Required]
        [StringLength(10)]
        public string TownName { get; set; }
    }
}
