using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Pvis.Biz.Models
{
    [Table("Waste_Prod", Schema = "code")]
    public class WasteProd
    {
        [Display(Name ="類別")]
        [Required]
        public string Code_Type { get; set; }
        [Display(Name ="代碼")]
        [Key]
        public string Code_no { get; set; }
        [Display(Name ="名稱")]
        public string Code_name { get; set; }
        public string is_delete { get; set; }
    }
}
