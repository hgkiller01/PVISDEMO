using System;
using System.Collections.Generic;
using System.Text;

namespace Pvis.Biz.CustomizedAttr
{
    public class CustomAttribute : Attribute
    {
        /// <summary>建構式</summary>
        public CustomAttribute()
        {
        }
        /// <summary>建構式</summary>
        /// <param name="Desc">描述</param>
        public CustomAttribute(string Desc)
        {
            Description = Desc;
        }
        /// <summary>描述文字</summary>
        public string Description { get; set; }

        /// <summary>排序屬性</summary>
        public int Sort { get; set; }

        /// <summary>覆寫轉字串</summary>
        public override string ToString()
        {
            return this.Description.ToString();
        }

    }
}
