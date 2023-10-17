using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Pvis.Biz.Models
{
    [Table("Compare_CLog", Schema = "Apply")]
    public class Compare_CLog
    {
        public int id { get; set; }
        /// <summary>
        /// 太陽能板流水號
        /// </summary>
        public int Spid { get; set; }
        /// <summary>
        /// 原本的太陽能板序號
        /// </summary>
        public string Sno { get; set; }
        /// <summary>
        /// 修改後的太陽能板序號
        /// </summary>
        public string Change_Sno { get; set; }
        /// <summary>
        /// 修改前外觀鋁框完整度(有1,無0)
        /// </summary>
        public string Al_frame { get; set; }
        /// <summary>
        /// 修改後外觀鋁框完整度(有1,無0)
        /// </summary>
        public string Change_frame { get; set; }
        public DateTime CreateDate { get; set; }
        public int Uid { get; set; }
    }
}
