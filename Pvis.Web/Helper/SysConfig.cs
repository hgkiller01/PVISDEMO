using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using System.IO;
using Pvis.Biz.Extension;
using Pvis.Biz.EmailSenderServices;

namespace Pvis.Web.Helper
{
    /// <summary>
    /// SysConfig 系統參數設定調整
    /// </summary>
    public class SysConfig
    {
        internal static string ConfgFilePath
        {
            get
            {
                return Path.Combine(Directory.GetCurrentDirectory(), "SysConfig.json");
            }
        }

        public static SysConfig Cfg
        {
            get
            {
                if (_Cfg != null) return _Cfg;
                if (!File.Exists(ConfgFilePath)) (new SysConfig()).Save(out _);

                try
                {
                    _Cfg = JsonConvert.DeserializeObject<SysConfig>(File.ReadAllText(ConfgFilePath));
                }
                catch
                {
                    _Cfg = new SysConfig()
                    {
                        MailBcc = new List<string>(),
                        ContactUsEmail = new List<string>(),
                        ReviewCcEmail = new List<string>(),
                        StmpServer = new EmailSettings()
                        {
                            MailServer = "127.0.0.1",
                            MailPort = 25,
                            SenderName = String.Empty,
                            Sender = String.Empty,
                            Password = String.Empty
                        }
                    };
                }
                return _Cfg;
            }
        }
        private static SysConfig _Cfg;

        /// <summary>
        /// 預設密件副本收件者
        /// </summary>
        public List<string> MailBcc { get; set; } = new List<string>();

        /// <summary>
        /// 聯絡我們 Email 設定
        /// </summary>
        public List<string> ContactUsEmail { get; set; } = new List<string>();

        /// <summary>
        /// 審查副本收件者
        /// </summary>
        public List<string> ReviewCcEmail { get; set; } = new List<string>();

        /// <summary>
        /// 系統名稱
        /// </summary>
        public string SiteName { get; set; } = "系統名稱尚未設定";

        /// <summary>
        /// 聯絡電話
        /// </summary>
        public string ContactPhone { get; set; }

        /// <summary>
        /// 諮詢電話
        /// </summary>
        public string SupportPhone { get; set; }

        /// <summary>
        /// 系統更新日期
        /// </summary>
        public DateTime UpdateDt { get; set; } = DateTime.Now;

        public string Sender => StmpServer.Sender;

        /// <summary>
        /// 郵件發信主機設定
        /// </summary>
        public EmailSettings StmpServer { get; set; } = new EmailSettings()
        {
            MailServer = "127.0.0.1",
            MailPort = 25,
            SenderName = String.Empty,
            Sender = String.Empty,
            Password = String.Empty
        };

        public bool Save(out List<string> Msg)
        {
            Msg = new List<string>();
            if (String.IsNullOrWhiteSpace(SiteName)) Msg.Add("系統名稱設定錯誤");
            if (MailBcc.Any(x => RegexUtilities.IsValidEmail(x) == false)) Msg.Add("預設密件副本收件者錯誤");
            if (ContactUsEmail.Any(x => RegexUtilities.IsValidEmail(x) == false)) Msg.Add("聯絡我們 Email 設定錯誤");
            if (ReviewCcEmail.Any(x => RegexUtilities.IsValidEmail(x) == false)) Msg.Add("審查副本收件者設定錯誤");
            if (RegexUtilities.IsValidEmail(StmpServer.Sender) == false) Msg.Add("預設寄件者 Email 設定錯誤");
            if (Msg.Any()) return false;
            SiteName = this.SiteName.ToSafehtml();
            try
            {
                File.WriteAllText(ConfgFilePath, JsonConvert.SerializeObject(this, Formatting.Indented));
            }
            catch
            {
                Msg.Add("寫入 JSON 設定檔過程發生異常!!");
                return false;
            }
            _Cfg = null;
            return true;
        }
    }
}
