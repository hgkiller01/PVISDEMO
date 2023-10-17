using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Pvis.Biz.Models;

namespace Pvis.Biz.Services
{
    public class FormFileUploadBusinessLayer
    {
        public static string GetSavePath(FormFileUpload f )
        {
            var P = f.FilePath.Split('/');
            if (P[0] == "~") P[0] = Directory.GetCurrentDirectory();
            string UploadRootPath = Path.Combine(Directory.GetCurrentDirectory(), "FormFileUpload");
            return Path.Combine(P);
        }

        public static void SetFilePath(FormFileUpload f)
        {
            if (string.IsNullOrEmpty(f.FileExtName)) throw new Exception("未設定存檔副檔名");
            f.FilePath = String.Join("/", new string[]{
                "~/FormFileUpload",
                DateTime.Now.Year.ToString(),
                ((int)f.DocType).ToString(),
                ((int)f.ItemType).ToString(),
                Guid.NewGuid().ToString().Replace("-", "")+"."+ f.FileExtName
            });
        }


        public static FormFileUpload FillFileData(Pvis.Biz.ViewModels.AttachmentViewModel avm, FormFileUpload f,int uid)
        {

            //f.AppId = avm.Pid.ToString();
            //f.ItemType = avm.Key;
            //f.DocType = avm.eDocType.AccountAppDoc;
            f.FileExtName = avm.FileExtName;
            f.OriginalFileName = avm.name;
            f.FileSize = avm.size;
            f.CreateUid = uid;
            f.CreateDt = DateTime.Now;
            f.ModUid = uid;
            f.ModDt = DateTime.Now;
            return f;
        }
    }
}
