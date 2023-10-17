using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using LinqKit;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Pvis.Biz.CommEnum;
using Pvis.Biz.Extension;
using Pvis.Biz.Models;
using Pvis.Biz.Services;
using Pvis.Biz.Member;
using Pvis.Biz.ViewModels;
using Pvis.Web.Helper;
using MimeDetective;

namespace Pvis.Web.Controller
{
    [Route("api/[controller]")]
    [ApiController]
    [PvisAuthorize(RoleList.Admin,RoleList.Epa)]
    public class NewsController : ControllerBase
    {

        private readonly DataDbContext _context;

        public NewsController(DataDbContext context)
        {
            _context = context;
        }

        [HttpPost]
        [Route("GetList")]
        public async Task<ActionResult<IEnumerable<News>>> GetList(NewsQry Qry)
        {
            var pred = PredicateBuilder.New<News>(true);

            if (!String.IsNullOrWhiteSpace(Qry.KeyWord))
            {
                Qry.KeyWord = Qry.KeyWord.Trim();
                pred = pred.And(x => x.Body.Contains(Qry.KeyWord));
            }
            if (Qry.IsEnable.HasValue)
            {
                pred = pred.And(x => x.IsEnable == Qry.IsEnable.Value);
            }
            return await _context.News.Where(pred).Take(1000).ToListAsync();
        }

        [HttpPost]
        [Route("GetItem")]
        public async Task<ActionResult<IEnumerable<News>>> GetItem(NewsQry Qry)
        {
            var N = _context.News.Where(x => x.Pid == Qry.Pid).FirstOrDefault();
            List<FormFileUpload> Att = null;
            if (N != null)
            {
                Att = await _context.FormFileUpload.Where(x =>
                    x.AppId == N.Pid.ToString() &&
                    x.DocType == eDocType.NewsDoc &&
                    x.ItemType == eItemType.None
                ).ToListAsync();
            }            
            return Ok(new
            {
                IsSuccess = (N != null),
                Rec = N ,
                AttList = Att.Select(x => new {
                   url = Url.Content(x.FilePath) ,
                   x.OriginalFileName
                })
            });
        }

        [HttpPost]
        [Route("Save")]
        public async Task<IActionResult> Save(News news)
        {
            var _EntityState = (news.Pid <= 0) ? EntityState.Added : EntityState.Modified;

            if (_EntityState == EntityState.Added)
            {
                news.CreateDt = DateTime.Now;
                news.CreateUid = User.GetUid();
            }

            news.ModDt = DateTime.Now;
            //news.Body = news.Body.ToSafehtml();
            news.ModUid = User.GetUid();

            _context.Entry(news).State = _EntityState;
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!NewsExists(news.Pid))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
            return Ok(new
            {
                Rec = news,
                IsAdded = (_EntityState == EntityState.Added)
            });
        }

        [HttpPost]
        [Route("SaveAttached")]
        public async Task<IActionResult> SaveAttached(NewsQry item)
        {
            var errors = new List<string>();

            if (item.Pid <= 0) errors.Add("主體資料尚未存檔不能上傳附件");

            if ((new string[] { "pdf" }).Contains(item.att.FileExtName) == false) errors.Add("附件只能使用 PDF 檔");
            //FileUploadErrorLog log = null;
            //if (FileCheck.IsAllowedExtension(item.att,"pdf", "最新消息後臺管理").Extension != "pdf")
            //{
            //    log = new FileUploadErrorLog()
            //    {
            //        DocType = eDocType.NewsDoc,
            //        ItemType = eItemType.None,
            //        Uid = User.GetUid(),
            //        ErrorDate = DateTime.Now,
            //        ErrorMessage = "上傳非PDF檔",
            //        IPAddress = Request.HttpContext.Connection.RemoteIpAddress.ToString(),
            //        OriginalFileName = item.att.name,
            //        UploadFunction = "最新消息後臺管理"
            //    };
            //    _context.FileUploadErrorLog.Add(log);
            //    errors.Add("附件只能使用 PDF 檔");
            //} 
                
            

            var F = _context.FormFileUpload.Where(x =>
                    x.AppId == item.Pid.ToString() &&
                    x.DocType == eDocType.NewsDoc &&
                    x.ItemType == eItemType.None
                ).FirstOrDefault();
            var _EntityState = (F == null) ? EntityState.Added : EntityState.Modified;
            F = F ?? new FormFileUpload()
            {
                AppId = item.Pid.ToString(),
                ItemType = eItemType.None,
                DocType = eDocType.NewsDoc,
                FileExtName = item.att.FileExtName,
                OriginalFileName = item.att.name,
                FileSize = item.att.size ,
                CreateUid = User.GetUid(),
                CreateDt = DateTime.Now
            };
            try
            {

                
                F.ModUid = User.GetUid();
                F.ModDt = DateTime.Now;
                if(FileCheck.IsAllowedExtension(item.att,F,_context,"pdf", "最新消息後臺管理", Request.HttpContext.Connection.RemoteIpAddress.ToString()))
                {
                    FormFileUploadBusinessLayer.SetFilePath(F);
                    Directory.CreateDirectory(Path.GetDirectoryName(FormFileUploadBusinessLayer.GetSavePath(F)));
                    System.IO.File.WriteAllBytes(FormFileUploadBusinessLayer.GetSavePath(F), item.att.GetContent());
                    if (_EntityState == EntityState.Added)
                    {
                        F.CreateDt = DateTime.Now;
                    }
                    F.ModDt = DateTime.Now;
                    _context.Entry(F).State = _EntityState;
                }
                else
                {
                    errors.Add("上傳檔案不正確");
                }
                
                if (errors.Any()) return BadRequest(new { errors });

            }
            catch(Exception ex)
            {
                FileCheck.WriteErrorFile(F,_context, ex.Message, "最新消息後臺管理", Request.HttpContext.Connection.RemoteIpAddress.ToString());
            }

            await _context.SaveChangesAsync();
            //FormFileUploadBusinessLayer.GetSavePath(F)
            return Ok();
        }

        [HttpPost]
        [Route("DeleteAttached")]
        public async Task<IActionResult> DeleteAttached(NewsQry item)
        {
            var F = _context.FormFileUpload.Where(x =>
                    x.AppId == item.Pid.ToString() &&
                    x.DocType == eDocType.NewsDoc &&
                    x.ItemType == eItemType.None
                ).FirstOrDefault();
            if (F == null)
            {
                return Ok();
            }
            var _SavePath = FormFileUploadBusinessLayer.GetSavePath(F);
            if (System.IO.File.Exists(_SavePath))
            {
                System.IO.File.Delete(_SavePath);
            }
            _context.Entry(F).State = EntityState.Deleted;
            await _context.SaveChangesAsync();
            return Ok();
        }

        [HttpPost]
        [Route("Delete")]
        public async Task<IActionResult> Delete(News news)
        {

            var _news = await _context.News.FindAsync(news.Pid);
            if (_news == null)
            {
                return NotFound();
            }

            _context.News.Remove(_news);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool NewsExists(int id)
        {
            return _context.News.Any(e => e.Pid == id);
        }

        public class NewsQry
        {
            public int Pid { get; set; }
            public string KeyWord { get; set; }
            public bool? IsEnable { get; set; }

            public AttachmentViewModel att { get; set; }

        }
    }
}
