using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Pvis.Biz.Models
{
    [Table("County", Schema = "code")]
    public partial class County
    {
        [StringLength(1)]
        public string CountyId { get; set; }
        [Required]
        [StringLength(5)]
        public string CountyName { get; set; }
    }
}
