using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Pvis.Biz.CommEnum;

namespace Pvis.Biz.ViewModels
{
    public class SPQry
    {
        public int Pid { get; set; }
        public string KeyWord { get; set; }
        public string Status { get; set; }
        public int? Pvid { get; set; }
        public Attached att { get; set; }
        public string Mode { get; set; }
        public string County { get; set; }
        public eItemType eItemType { get; set; }
        public int Page { get; set; } = 1;
        public int TotalCount { get; set; } = 0;
        public class Attached
        {
            public String name { get; set; }
            public long size { get; set; }
            public string body
            {
                set
                {
                    this.Content = Convert.FromBase64String(value);
                }
            }
            public String mimetype { get; set; }
            public string FileExtName
            {
                get
                {
                    return Path.GetExtension(name).Replace(".", "").ToLower();
                }
            }
            private byte[] Content { get; set; }

            public byte[] GetContent()
            {
                return this.Content;
            }
        }
    }
}
