using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Pvis.Biz.ViewModels;
using System.IO;
using Pvis.Biz.CommEnum;
using MimeDetective;
using Pvis.Biz.Models;

namespace Pvis.Web.Helper
{
    public class FileCheck
    {
        public static bool IsAllowedExtension(AttachmentViewModel attachment,FormFileUpload ffu, DataDbContext context,
            string extType,string FunctionName,string IP)
        {
            FileType fileType = attachment.GetContent().GetFileType();
            if(fileType.Extension != extType)
            {

                FileUploadErrorLog log = new FileUploadErrorLog()
                {
                    DocType = ffu.DocType,
                    ErrorMessage = "上傳非" + extType + "檔案",
                    ErrorDate = DateTime.Now,
                    IPAddress = IP,
                    ItemType = ffu.ItemType,
                    OriginalFileName = ffu.OriginalFileName,
                    UploadFunction = FunctionName,
                    Uid = ffu.CreateUid
                };
                context.FileUploadErrorLog.Add(log);
                context.SaveChanges();
                return false;
            }
            else
            {
                return true;
            }
        }
        public static void WriteErrorFile(FormFileUpload ffu, DataDbContext context, string errorMessage, string FunctionName, string IP)
        {
            FileUploadErrorLog log = new FileUploadErrorLog()
            {
                Uid = ffu.CreateUid,
                DocType = ffu.DocType,
                ErrorDate = DateTime.Today,
                ErrorMessage = errorMessage,
                IPAddress = IP,
                ItemType = ffu.ItemType,
                OriginalFileName = ffu.OriginalFileName,
                UploadFunction = FunctionName
            };
            context.FileUploadErrorLog.Add(log);
            context.SaveChanges();
        }
        
    }
}
