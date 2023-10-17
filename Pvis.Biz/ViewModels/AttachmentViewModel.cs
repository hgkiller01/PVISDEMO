using System;
using System.IO;

namespace Pvis.Biz.ViewModels
{

    /// <summary>
    /// 接收前端附件上傳的基本物件
    /// </summary>
    public class AttachmentViewModel
    {
        public String name { get; set; }
        public long size { get; set; }
        public string body
        {
            set
            {
                Content = Convert.FromBase64String(value);
            }
        }
        public String mimetype { get; set; }
        public string FileExtName
        {
            get
            {
                if (string.IsNullOrWhiteSpace(name)) return null;
                return Path.GetExtension(name).Replace(".", "").ToLower();
            }
        }
        private byte[] Content { get; set; }

        public byte[] GetContent()
        {
            return Content;
        }
    }
}
