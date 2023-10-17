using System.Collections.Generic;
using System.IO;

namespace Pvis.Web.Helper
{
    /// <summary>廢太陽光電板清除處理機構列表</summary>
    public class Organ
    {
        /// <summary>機構類型</summary>
        public enum Type
        {
            clean = 1,
            treat = 2
        }
        /// <summary>機構類型</summary>
        public Type type { get; set; }
        /// <summary>資料</summary>
        public string data { get; set; } = "";

        public bool Save(out List<string> Errors)
        {
            Errors = new List<string>();
            var path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "mgr", $"organ{type}.json");

            try
            {
                File.WriteAllText(path, $"{{\"data\":{ data }}}");
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
