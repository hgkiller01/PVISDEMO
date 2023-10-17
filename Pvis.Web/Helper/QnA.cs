using System.Collections.Generic;
using System.IO;

namespace Pvis.Web.Helper
{
    /// <summary>問與答</summary>
    public class QnA
    {
        /// <summary>廢太陽光電板回收處理機制</summary>
        public string C0 { get; set; }
        /// <summary>帳號登入相關問題</summary>
        public string C1 { get; set; }
        /// <summary>線上登記排出廢太陽光電板相關問題</summary>
        public string C2 { get; set; }

        public bool Save(out List<string> Errors)
        {
            Errors = new List<string>();
            var path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "mgr", "qna.json");

            try
            {
                File.WriteAllText(path, $"{{\"C0\":{ C0 },\"C1\":{ C1 },\"C2\":{ C2 }}}");
            }
            catch
            {
                Errors.Add("寫入 JSON 過程發生異常!!");
                return false;
            }
            return true;
        }
    }
}
