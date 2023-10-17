using System.IO;
using Newtonsoft.Json;

namespace Pvis.Biz.EmailSenderServices
{
    public class EmailSettings
    {
        private string _CfgFile;

        public string MailServer { get; set; }
        public int MailPort { get; set; } = 25;
        public string SenderName { get; set; }
        public string Sender { get; set; }
        public string Password { get; set; }

        public bool UseSSL
        {
            get
            {
                return MailPort != 25;
            }
        }

        public void CfgFile(string confgFilePath)
        {
            _CfgFile = confgFilePath;
            Reload();
        }

        public void Reload()
        {
            if (!File.Exists(_CfgFile)) return;
            TmpConfig _Cfg = null;
            try
            {
                _Cfg = JsonConvert.DeserializeObject<TmpConfig>(File.ReadAllText(_CfgFile));
            }
            catch {
                return;
            }
            this.MailServer = _Cfg.StmpServer.MailServer;
            this.MailPort = _Cfg.StmpServer.MailPort;
            this.SenderName = _Cfg.StmpServer.SenderName;
            this.MailServer = _Cfg.StmpServer.MailServer;
            this.Sender = _Cfg.StmpServer.Sender;
            this.Password = _Cfg.StmpServer.Password;

        }

        class TmpConfig
        {
            public EmailSettings StmpServer { get; set; }
        }
    }
}
