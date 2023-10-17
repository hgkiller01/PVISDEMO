using System;
using System.Collections.Generic;
using System.Linq;
using LinqKit;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Pvis.Biz.CommEnum;
using Pvis.Biz.Extension;
using Pvis.Biz.Models;
using Pvis.Biz.Member;
using System.IO;
using Pvis.Biz.Services;
using AutoMapper;
using Pvis.Web.Helper;
using Pvis.Biz.ViewModels;

namespace Pvis.Web.Controller
{
    [Route("api/[controller]")]
    [ApiController]
    [PvisAuthorize(RoleList.Admin, RoleList.Company)]
    public class UserSpInfoController : ControllerBase
    {
        private readonly DataDbContext _context;

        public UserSpInfoController(DataDbContext context)
        {
            _context = context;
        }

        //同步join子集
        //[HttpPost]
        //[Route("Load")]
        //public  IEnumerable<UserSpInfo> Load()
        //{
        //    var SpResult=  _context.UserSpInfo.ToList();
        //    var PvResult =  _context.UserPvInfo.ToList();
        //    var newResault = SpResult.Join(PvResult, Sp => Sp.Pvid, Pv => Pv.Pid, (Sp, Pv) => Sp.LoadPvInfo(Pv));
        //    return newResault;
        //}

        [HttpPost]
        [Route("GetList")]
        public async Task<ActionResult<SpInfosData>> GetList(SPQry Qry)
        {
            var pred = PredicateBuilder.New<UserSpInfo>(true);
            if (User.HasRole(RoleList.Company)) 
                pred = pred.And(x => x.Uid == User.GetUid());
            if (!String.IsNullOrWhiteSpace(Qry.KeyWord))
            {
                Qry.KeyWord = Qry.KeyWord.Trim();
                pred = pred.And(x => x.Sno.Contains(Qry.KeyWord));
            }
            if (!string.IsNullOrEmpty(Qry.Status))
            {
                pred = pred.And(x => x.Status == Qry.Status);
            }
            if (Qry.Pvid.HasValue) { pred = pred.And(x => x.Pvid == Qry.Pvid); }
            SpInfosData spInfo = new SpInfosData();
            spInfo.TotalDataCount = _context.UserSpInfo.Where(pred).Count();
            spInfo.Count = Convert.ToInt32(Math.Ceiling(spInfo.TotalDataCount / 10.0));
            spInfo.Data = await _context.UserSpInfo.Where(pred).Skip((Qry.Page - 1) * 10).Take(10).ToListAsync();
            return spInfo;
        }

        //[HttpPost]
        //[Route("Load")]
        //public async Task<ActionResult<IEnumerable<UserSpInfo>>> Load()
        //{
        //    if (User.HasRole(RoleList.Company))
        //        return await _context.UserSpInfo.Where(x => x.Uid == User.GetUid()).ToListAsync();
        //    else
        //        return await _context.UserSpInfo.ToListAsync();
        //}

        [HttpPost]
        [Route("GetItem")]
        public async Task<ActionResult<UserSpInfo>> GetItemAsync(Biz.ViewModels.SPQry SP)
        {
            var _Rec = _context.UserSpInfo.Where(x => x.Pid == SP.Pid).FirstOrDefault();
            if (User.HasRole(RoleList.Company)) _Rec = _context.UserSpInfo.Where(x => x.Pid == SP.Pid && x.Uid == User.GetUid()).FirstOrDefault();
            List<FormFileUpload> Att = null;
            if (_Rec != null && _Rec.Pid > 0)
            {
                Att = await _context.FormFileUpload.Where(x =>
                    x.AppId == _Rec.Pid.ToString() &&
                    x.DocType == eDocType.UserSpInfoDoc &&
                    x.ItemType == eItemType.None
                ).ToListAsync();

                _Rec.PV = await _context.UserPvInfo.Where(x => x.Pid == SP.Pvid).FirstOrDefaultAsync();

                #region 案場排出登記表-審查確認通過狀態
                var query = from a in _context.Set<ScrapBooking>()
                            join b in _context.Set<ScrapBookingReview>()
                                on a.Pid equals b.SBPid
                            where a.Pid == _Rec.Sbid && new[] { "Y1", "Y2" }.Contains(b.Status)
                            select new { a.Pid };
                _Rec.ReviewStatus = await query.AnyAsync();
                #endregion
            }

            return Ok(new
            {
                IsSuccess = (_Rec != null),
                Rec = _Rec,
                AttList = Att.Select(x => new
                {
                    url = Url.Content(x.FilePath),
                    x.OriginalFileName
                })
            });
        }

        [HttpPost]
        [Route("Save")]
        public async Task<IActionResult> Save(ViewSPmodel USPA)
        {
            UserSpInfo USP = USPA.Rec;

            var errors = new List<string>();
            if (USP.Style == "9" && string.IsNullOrEmpty(USP.StyleDesc)) errors.Add("請說明模組樣態(10字以內)");
            if (USP.Hasno == "0" && string.IsNullOrWhiteSpace(USP.Memo)) errors.Add("無太陽光電板序號，請填寫備註說明");
            if (SPCntOver(USP.Pvid)) errors.Add("太陽光電板數量，已超過設備總量");
            if (errors.Count > 0) { return BadRequest(new { IsSuccess = false, errors }); }

            var _EntityState = (USP.Pid <= 0) ? EntityState.Added : EntityState.Modified;
            if (_EntityState == EntityState.Added)
            {
                if (UserSpInfoExists(USP.Sno)) errors.Add("此太陽光電板序號已設定過");
                if (errors.Count > 0) { return BadRequest(new { IsSuccess = false, errors }); }
                if (USP.Hasno == "1" && AllSpInfoExists(USP.Sno) && USPA.Attachment.Count == 0) errors.Add("已有其他案場申請過該序號，請上傳該序號的太陽光電板設備標籤照片");
                if (errors.Count > 0) { return BadRequest(new { IsSuccess = false, errors }); }
                USP.Createdate = DateTime.Now;
            }
            USP.Uid = User.GetUid();
            //USP.Status = "1";

            using (var transaction = _context.Database.BeginTransaction())
            {
                _context.Entry(USP).State = _EntityState;
                await _context.SaveChangesAsync();


                foreach (var item in USPA.Attachment)
                {
                    if (_EntityState != EntityState.Added) { await DeleteAttached(new Biz.ViewModels.SPQry { Pid = USPA.Rec.Pid }); }
                    var F = FormFileUploadBusinessLayer.FillFileData(item.Value, new FormFileUpload() { AppId = USP.Pid.ToString(), DocType = eDocType.UserSpInfoDoc, ItemType = item.Key }, User.GetUid());
                    if (FileCheck.IsAllowedExtension(item.Value, F,_context ,"pdf", "太陽光電板資料維護", Request.HttpContext.Connection.RemoteIpAddress.ToString()))
                    {
                        try
                        {
                            FormFileUploadBusinessLayer.SetFilePath(F);
                            Directory.CreateDirectory(Path.GetDirectoryName(FormFileUploadBusinessLayer.GetSavePath(F)));
                            System.IO.File.WriteAllBytes(FormFileUploadBusinessLayer.GetSavePath(F), item.Value.GetContent());
                            _context.Add(F).State = EntityState.Added;
                        }
                        catch (Exception ex)
                        {
                            FileCheck.WriteErrorFile(F,_context ,ex.Message, "太陽光電板資料維護", Request.HttpContext.Connection.RemoteIpAddress.ToString());
                        }
                        await _context.SaveChangesAsync();
                    }

                }



                await _context.SaveChangesAsync();
                transaction.Commit();

            }

            return Ok(new
            {
                Rec = USP,
                IsAdded = (_EntityState == EntityState.Added)
            });
        }

        [HttpPost]
        [Route("Delete")]
        public async Task<IActionResult> Delete(UserSpInfo UserSpInfo)
        {
            var errors = new List<string>();
            var _UserSpInfo = await _context.UserSpInfo.FindAsync(UserSpInfo.Pid);
            if (_UserSpInfo == null)
            {
                return NotFound();
            }
            //移除附件
            var _SQ = new Biz.ViewModels.SPQry { Pid = UserSpInfo.Pid };
            await DeleteAttached(_SQ);
            if (UserSpInfo.Sbid > 0) errors.Add("此太陽光電板已被使用，無法刪除!");
            if (errors.Count > 0) { return BadRequest(new { IsSuccess = false, errors }); }

            _context.UserSpInfo.Remove(_UserSpInfo);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpPost]
        [Route("SaveAttached")]
        public async Task<IActionResult> SaveAttached(Biz.ViewModels.SPQry item)
        {
            var F = _context.FormFileUpload.Where(x =>
                    x.AppId == item.Pid.ToString() &&
                    x.DocType == eDocType.UserSpInfoDoc &&
                    x.ItemType == eItemType.None
                ).FirstOrDefault();
            var _EntityState = (F == null) ? EntityState.Added : EntityState.Modified;
            F = F ?? new FormFileUpload()
            {
                AppId = item.Pid.ToString(),
                ItemType = eItemType.None,
                DocType = eDocType.UserSpInfoDoc,
                FileExtName = item.att.FileExtName,
                OriginalFileName = item.att.name,
                FileSize = item.att.size,
                CreateUid = User.GetUid(),
                CreateDt = DateTime.Now
            };
            F.ModUid = User.GetUid();
            F.ModDt = DateTime.Now;
            FormFileUploadBusinessLayer.SetFilePath(F);
            Directory.CreateDirectory(Path.GetDirectoryName(FormFileUploadBusinessLayer.GetSavePath(F)));
            System.IO.File.WriteAllBytes(FormFileUploadBusinessLayer.GetSavePath(F), item.att.GetContent());
            if (_EntityState == EntityState.Added)
            {
                F.CreateDt = DateTime.Now;
            }
            F.ModDt = DateTime.Now;
            _context.Entry(F).State = _EntityState;
            await _context.SaveChangesAsync();
            return Ok(FormFileUploadBusinessLayer.GetSavePath(F));
        }

        [HttpPost]
        [Route("DeleteAttached")]
        public async Task<IActionResult> DeleteAttached(Biz.ViewModels.SPQry item)
        {
            var F = _context.FormFileUpload.Where(x =>
                    x.AppId == item.Pid.ToString() &&
                    x.DocType == eDocType.UserSpInfoDoc &&
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

        private bool UserSpInfoExists(int id)
        {
            return _context.UserSpInfo.Any(e => e.Pid == id);
        }

        private bool UserSpInfoExists(string _Sno)
        {
            return _context.UserSpInfo.Any(e => e.Uid == User.GetUid() && e.Sno.Equals(_Sno));
        }

        private bool SPCntOver(int? _Pvid)
        {
            int pvqty;
            int cntqty;
            pvqty = _context.UserPvInfo.Where(x => x.Pid == _Pvid).Sum(x => x.SpQty);
            cntqty = _context.UserSpInfo.Where(x => x.Pvid == _Pvid && x.Status == "1").Count();

            return (cntqty > pvqty);
        }

        private bool AllSpInfoExists(string _Sno)
        {
            return _context.UserSpInfo.AsEnumerable().Any(e => e.Sno.Equals(_Sno, StringComparison.OrdinalIgnoreCase));
        }

        public class ViewSPmodel
        {
            public UserSpInfo Rec { get; set; }
            public Dictionary<eItemType, Pvis.Biz.ViewModels.AttachmentViewModel> Attachment { internal get; set; }
        }
        public class SpInfosData
        {
            public List<UserSpInfo> Data { get; set; }
            public int Count { get; set; } = 0;
            public int TotalDataCount { get; set; } = 0;
        }

    }

}
