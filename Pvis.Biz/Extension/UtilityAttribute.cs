using System;
using System.Collections.Generic;
using System.Web;
using System.Text.RegularExpressions;


namespace ERI.Utility.Extensions
{
    /// <summary>自訂屬性</summary>
    [AttributeUsage(
        AttributeTargets.Class |
        AttributeTargets.Field |
        AttributeTargets.Property|
        AttributeTargets.Enum
        )]
  public  class CustomAttribute : Attribute
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


    /// <summary>表單自訂欄位屬性</summary>
    [AttributeUsage(
        AttributeTargets.Field |
        AttributeTargets.Property)]
    public class FormAttr : Attribute
    {
        /// <summary>建構式</summary>
        public FormAttr()
        {

        }

        /// <summary>是否進行繫結</summary>
        public bool IsDataBind = true;

        /// <summary>是否鍵值欄位</summary>
        public bool IsPk = false;

        /// <summary>是否為識別欄位(同一類別中只允許出限定義一次)</summary>
        public bool IsIDENTITY = false;        

        /// <summary>資料驗證Regex規則式</summary>
        public string RegexString = ".";

        /// <summary>資料驗證Regex規則提示</summary>
        public string RegexToolTip = "";

        /// <summary>資料驗證</summary>
        public bool VerifyRegex(string VerifyData)
        {
            if (RegexString == ".") return true;
            Regex Reg = new Regex(RegexString, RegexOptions.Singleline);
            return Reg.IsMatch(VerifyData);
        }
    }

}