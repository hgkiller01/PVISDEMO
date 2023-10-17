using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using ERI.Utility.Extensions;
using LinqKit;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Pvis.Biz.CommEnum;
using Pvis.Biz.EmailSenderServices;
using Pvis.Biz.Extension;
using Pvis.Biz.Member;
using Pvis.Biz.Models;
using Pvis.Biz.Services;
using Pvis.Web.Helper;
using Pvis.Biz.ViewModels;

namespace Pvis.Web.Controller
{
    [Route("api/[controller]")]
    [ApiController]
    [ValidateAntiForgeryToken]
    [PvisAuthorize(RoleList.Admin, RoleList.Auditor, RoleList.Company)]
    public class ScrapBookingController : ControllerBase
    {
        private readonly DataDbContext _context;
        private readonly IEmailSender _email;
        private static IMapper mapper;

        public ScrapBookingController(DataDbContext context, IEmailSender email)
        {
            _context = context;
            _email = email;

            var configuration = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<ApplyPvInfo, UserPvInfo>();
            });
            mapper = configuration.CreateMapper();
        }
        [HttpPost]
        [Route("GetCounty")]
        public async Task<List<string>> GetCounty()
        {
            return await _context.County.Select(x => x.CountyName).ToListAsync();            
        }
        [HttpPost]
        [Route("GetList")]
        public async Task<ActionResult<IEnumerable<ScrapBooking>>> GetList(SPQry Qry)
        {
            var pb = PredicateBuilder.New<ScrapBooking>(true);
            if (!User.HasRole(RoleList.Admin, RoleList.Auditor, RoleList.Epa))
            {
                if (User.HasRole(RoleList.Company) || Qry.Mode == "Apply")
                    pb = pb.And(x => x.Uid == User.GetUid());
            }

            if (string.IsNullOrWhiteSpace(Qry.Mode)) pb = pb.And(x => x.Status != "1");

            if (!string.IsNullOrWhiteSpace(Qry.KeyWord))
            {
                Qry.KeyWord = Qry.KeyWord.Trim();
                if (Qry.KeyWord.TryParseInt() > 0 && Qry.KeyWord.TryParseInt() < 2009999999)
                {
                    pb = pb.And(x => x.Bookingno.Contains(Qry.KeyWord));
                }
                else if (Qry.KeyWord.Contains("@"))
                {
                    pb = pb.And(x => x.Email.Contains(Qry.KeyWord));
                }
                else
                {
                    pb = pb.And(x => x.Contact.Contains(Qry.KeyWord));
                }
            }
            if (!string.IsNullOrEmpty(Qry.Status))
            {
                pb = pb.And(x => x.Status == Qry.Status);
            }

            var sb = await _context.ScrapBooking.Where(pb).Take(1000).ToListAsync();
            var usa = await _context.UserStoreAddress.ToListAsync();
            var companys = AuthHelper.GetUserList().ToList();
            foreach (var item in sb)
            {
                
                item.SBCity = usa.FirstOrDefault(x => x.Pid == item.Uspid).Storeaddr.Substring(0, 3);
                item.Qty = await _context.UserSpInfo.Where(x => x.Sbid == item.Pid).CountAsync();
            }
            if (!string.IsNullOrEmpty(Qry.County))
            {
                sb = sb.Where(x => x.SBCity == Qry.County).ToList();
            }
            sb = sb.OrderByDescending(x => x.Bookingno).ToList();
            return sb;
        }

        [HttpPost]
        [Route("Load")]
        public async Task<ActionResult<IEnumerable<ScrapBooking>>> Load()
        {
            return await _context.ScrapBooking.Where(x => x.Uid == User.GetUid()).Take(1000).ToListAsync();
        }

        [HttpPost]
        [Route("GetItem")]
        public async Task<ActionResult<ScrapBooking>> GetItem(SPQry Qry)
        {
            var _Rec = _context.ScrapBooking.Where(x => x.Pid == Qry.Pid).FirstOrDefault();
            List<FormFileUpload> Att = null;
            if (_Rec != null && _Rec.Pid > 0)
            {
                Att = await _context.FormFileUpload.Where(x =>
                    x.AppId == _Rec.Pid.ToString() &&
                    x.DocType == eDocType.ScrapBookingDoc &&
                    x.ItemType == eItemType.None
                ).ToListAsync();
                //var companyUser = AuthHelper.GetUserQuery().Where(x => x.Uid == _Rec.Uid).FirstOrDefault();
                //var att2 = _context.FormFileUpload.Where(x => x.AppId == companyUser.AppPid.ToString() &&
                //x.DocType == eDocType.AccountAppDoc && x.ItemType == eItemType.AccountApp_LetterOfAgreement)
                //    .FirstOrDefault();
                //Att.Add(att2);
                _Rec.SP = await _context.UserSpInfo.Where(x => x.Sbid == Qry.Pid).ToListAsync();
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
        [Route("GetView")]
        public async Task<ActionResult<ScrapBooking>> View(SPQry Qry)
        {
            var _Rec = _context.ScrapBooking.Where(x => x.Pid == Qry.Pid).FirstOrDefault();
            List<FormFileUpload> Att = null;
            List<ScrapBookingReview> ListSBR = null;

            if (_Rec != null && _Rec.Pid > 0)
            {
                var SBPid = _Rec.Pid;

                Att = await _context.FormFileUpload.Where(x =>
                    x.AppId == SBPid.ToString() &&
                    x.DocType == eDocType.ScrapBookingDoc &&
                    x.ItemType == eItemType.None
                ).ToListAsync();

                ListSBR = await _context.ScrapBookingReview.Where(x => x.SBPid == SBPid).OrderByDescending(x => x.CKDate).ThenByDescending(x => x.Pid).ToListAsync();
                ListSBR.ForEach(x => x.CL = _context.ClearLocation.SingleOrDefault(y => y.Pid == x.CLPid));

                //var ListUSI = await _context.UserSpInfo.Where(x => x.Sbid == SBPid).OrderBy(x => x.Pvno).ToListAsync();
                var ListUSI = (await _context.UserSpInfo.Where(x => x.Sbid == SBPid).ToListAsync())
                    .OrderBy(x => x.Pvno).ToList();
                if (_Rec.Status == "Y1" || _Rec.Status == "Y2")
                {
                    var ListAPI = await _context.ApplyPvInfo.Where(x => x.SBPid == SBPid).ToListAsync();
                    _Rec.PV = mapper.Map<List<ApplyPvInfo>, List<UserPvInfo>>(ListAPI);
                }
                else
                {
                    var pvid = ListUSI.Select(x => x.Pvid).Distinct();
                    _Rec.PV = await _context.UserPvInfo.Where(x => pvid.Contains(x.Pid)).ToListAsync();
                }
                _Rec.PV.ForEach(x => x.SP = ListUSI.Where(y => y.Pvid == x.Pid).OrderBy(y => y.Sno).ToList());
                foreach (var i in _Rec.PV)
                {
                    var data = (from upv in _context.UserPvInfo
                                join sp in _context.UserSpInfo on upv.Pid equals sp.Pvid
                                join scarp in _context.ScrapBooking on sp.Sbid equals scarp.Pid
                                where upv.Pid == i.Pid
                                select scarp).ToList();
                    i.ReviewQty = data.Where(x => x.Status == "Y1" || x.Status == "Y2").Count();
                    i.TotalPvCount = (from upv in _context.UserPvInfo
                                      join sp in _context.UserSpInfo on upv.Pid equals sp.Pvid
                                      where upv.Pid == i.Pid
                                      select sp).Count();
                    //i.ReviewQty = getReviewQty(i.Pid);

                    i.FileApplyDoc = await _context.FormFileUpload.Where(x => x.AppId == i.Pid.ToString()
                        && x.DocType == eDocType.UserPvInfoDoc
                        && x.ItemType == eItemType.ApplyDoc
                    ).ToListAsync();

                    i.FileProvDoc = await _context.FormFileUpload.Where(x => x.AppId == i.Pid.ToString()
                        && x.DocType == eDocType.UserPvInfoDoc
                        && x.ItemType == eItemType.ProvDoc
                    ).ToListAsync();

                    i.FileAgreement = await _context.FormFileUpload.Where(x => x.AppId == i.Pid.ToString()
                        && x.DocType == eDocType.UserPvInfoDoc
                        && x.ItemType == eItemType.AccountApp_LetterOfAgreement
                    ).ToListAsync();

                    i.FilePvSnDoc = await _context.FormFileUpload.Where(x => x.AppId == i.Pid.ToString()
                        && x.DocType == eDocType.UserPvInfoDoc
                        && x.ItemType == eItemType.PvSnDoc).ToListAsync();

                    foreach (var j in i.SP)
                    {
                        j.File = await _context.FormFileUpload.Where(x => x.AppId == j.Pid.ToString()
                            && x.DocType == eDocType.UserSpInfoDoc
                            && x.ItemType == eItemType.None
                        ).ToListAsync();
                    }
                }

                _Rec.USAddr = await _context.UserStoreAddress.Where(x => x.Pid == _Rec.Uspid).ToListAsync();
                _Rec.Qty = ListUSI.Count;
            }

            return Ok(new
            {
                IsSuccess = (_Rec != null),
                Rec = _Rec,
                AttList = Att.Select(x => new
                {
                    url = Url.Content(x.FilePath),
                    x.OriginalFileName
                }),
                ReviewList = ListSBR
            });
        }

        [HttpPost]
        [Route("Save")]
        public async Task<IActionResult> Save(ViewSBmodel SBA)
        {
            var SB = SBA.Rec;
            var _EntityState = SB.Pid > 0 ? EntityState.Modified : EntityState.Added;
            
            if (_EntityState == EntityState.Added)
            {
                var ymd = (DateTime.Now.Year - 1911) + DateTime.Now.ToString("MMdd");
                var cnt = "001";
                var bkno = "";
                if (_context.ScrapBooking.Count() > 0)
                {
                    bkno = _context.ScrapBooking.AsEnumerable().Last().Bookingno;
                }
                if (bkno.Substring(0, 7) == ymd)
                {
                    cnt = (bkno.Substring(7, 3).TryParseInt() + 1).ToString("000");
                }
                SB.Appdate = DateTime.Now;
                SB.Bookingno = ymd + cnt;
                SB.Status = "1";
            }




            //if (errors.Count > 0) { return BadRequest(new { IsSuccess = false, errors }); }

            if (_EntityState == EntityState.Modified && SB.Status == "S")
            {
                var errors = new List<string>();
                var hasAtt = false;
                if (SB.Pid > 0) { hasAtt = AttachmentExists(SB.Pid.ToString()); }
                if (SPlistNotHasno(SB.SP) && SBA.Attachment.Count == 0 && !hasAtt) errors.Add("選擇的太陽光電板序號有自行編號者，請上傳切結書。");
                if (SameSPlistNo(SB.SP) && SBA.Attachment.Count == 0 && !hasAtt) errors.Add("選擇的太陽光電板有重複序號者，請上傳切結書。");
                if (SB.Uspid == 0) errors.Add("未選擇存放地點");
                if (SB.SP.Count == 0) errors.Add("未選擇任何太陽光電板");
                if (errors.Count > 0) { return BadRequest(new { IsSuccess = false, errors }); }
            }
            SB.Uid = User.GetUid();

            using (var transaction = _context.Database.BeginTransaction())
            {
                _context.Entry(SB).State = _EntityState;
                await _context.SaveChangesAsync();

                foreach (var item in SBA.Attachment)
                {
                    if (_EntityState != EntityState.Added) { await DeleteAttached(new SPQry { Pid = SBA.Rec.Pid }); }
                    var F = FormFileUploadBusinessLayer.FillFileData(item.Value, new FormFileUpload() { AppId = SB.Pid.ToString(), DocType = eDocType.ScrapBookingDoc, ItemType = item.Key }, User.GetUid());
                    if (FileCheck.IsAllowedExtension(item.Value, F,_context, "pdf", "案場排出登記表", Request.HttpContext.Connection.RemoteIpAddress.ToString()))
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
                            FileCheck.WriteErrorFile(F,_context, ex.Message, "案場排出登記表", Request.HttpContext.Connection.RemoteIpAddress.ToString());
                        }
                    }
                }

                _context.UserSpInfo.Where(x => x.Sbid == SB.Pid && !SB.SP.Select(y => y.Pid).Contains(x.Pid)).ToList().ForEach(x =>
                    {
                        x.Sbid = 0;
                        _context.Add(x).State = EntityState.Modified;
                    });

                SB.SP.Distinct().ToList().ForEach(x =>
                    {
                        x.Sbid = SB.Pid;
                        if (_context.UserSpInfo.Any(z => z.Pid == x.Pid && (z.Sbid == 0 || z.Sbid == null || z.Sbid == SB.Pid)))
                        {
                            _context.Add(x).State = EntityState.Modified;
                        }
                    }
                );

                await _context.SaveChangesAsync();
                transaction.Commit();
            }

            SB.Qty = SB.SP.Distinct().Count();

            if (SB.Status == "S")
            {
                var USA = await _context.UserStoreAddress.FindAsync(SB.Uspid);
                await SendMail(SB, USA);
            }

            return Ok(new
            {
                Rec = SB,
                IsAdded = (_EntityState == EntityState.Added)
            });
        }

        /// <summary>寄送郵件</summary>
        /// <param name="SB">案場排出登記表</param>
        /// <param name="USA">存放地點</param>
        private async Task SendMail(ScrapBooking SB, UserStoreAddress USA)
        {
            var message = $@"{SB.Contact} 先生/小姐，您好<br><br>
我們已收到您的案場排出登記資料，內容簡述如下<br>
申請編號：{SB.Bookingno}<br>
存放地點：{USA.Storeaddr}<br>
詳細內容，請至{SysConfig.Cfg.SiteName}查詢，感謝您<br>
【此郵件為系統自動發送，請勿回信！】";

            await _email.SendEmailAsync(
                new List<string> { SB.Email },
                SysConfig.Cfg.ReviewCcEmail.Union(SysConfig.Cfg.MailBcc),
                null,
                $"{SB.Bookingno}廢太陽光電板案場排出登記通知信",
                message
                );
        }

        [HttpPost]
        [Route("SaveAttached")]
        public async Task<IActionResult> SaveAttached(SPQry item)
        {
            var F = _context.FormFileUpload.Where(x =>
                    x.AppId == item.Pid.ToString() &&
                    x.DocType == eDocType.ScrapBookingDoc &&
                    x.ItemType == eItemType.None
                ).FirstOrDefault();
            var _EntityState = (F == null) ? EntityState.Added : EntityState.Modified;
            F = F ?? new FormFileUpload()
            {
                AppId = item.Pid.ToString(),
                ItemType = eItemType.None,
                DocType = eDocType.ScrapBookingDoc,
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
        public async Task<IActionResult> DeleteAttached(SPQry item)
        {
            var F = _context.FormFileUpload.Where(x =>
                    x.AppId == item.Pid.ToString() &&
                    x.DocType == eDocType.ScrapBookingDoc &&
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
        public async Task<IActionResult> Delete(ScrapBooking SB)
        {
            var _ScrapBooking = await _context.ScrapBooking.FindAsync(SB.Pid);
            if (_ScrapBooking == null)
            {
                return NotFound();
            }
            //移除附件
            SPQry _SQ = new SPQry { Pid = SB.Pid };
            await DeleteAttached(_SQ);


            //移除使用的太陽光電板
            _context.UserSpInfo
                .Where(x => x.Sbid == SB.Pid)
                .ToList().ForEach(x =>
                {
                    x.Sbid = 0;
                    _context.Add(x).State = EntityState.Modified;
                });


            _context.ScrapBooking.Remove(_ScrapBooking);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpPost]
        [Route("GetUspidOptions")]
        public async Task<ActionResult<List<UserStoAddrOptions>>> GetUspidOptions()
        {
            return (await _context.UserStoreAddress.Where(x => x.Uid == User.GetUid() && x.Status == "1").Select(x => new UserStoAddrOptions() { Pid = x.Pid, Storeaddr = x.Storeaddr }).ToListAsync());
        }

        [HttpPost]
        [Route("GetSpOptions")]
        public async Task<ActionResult<List<UserSpInfo>>> GetSpOptions()
        {
            var uid = User.GetUid();
            
            return await _context.UserSpInfo.Where(x => x.Uid == User.GetUid() && x.Status == "0").ToListAsync();
        }

        [HttpPost]
        [Route("GetCLOptions")]
        public async Task<ActionResult<List<ClearLocation>>> GetCLOptions()
        {
            return await _context.ClearLocation.Where(x => x.Status.Value).ToListAsync();
        }

        [HttpPost]
        [Route("GetUserInfo")]
        public UserInfo GetUserInfo()
        {
            UserInfo _UIF = new UserInfo();
            _UIF.Contact = User.GetCurrentUser().DisplayName; ;
            _UIF.Tel = User.GetCurrentUser().PhoneNumber;
            _UIF.Email = User.GetCurrentUser().Email;
            return (_UIF);

        }



        private bool ScrapBookingExists(int id)
        {
            return _context.ScrapBooking.Any(e => e.Pid == id);
        }

        private bool AttachmentExists(string appid)
        {
            return _context.FormFileUpload.Any(e => e.AppId == appid && e.DocType == eDocType.ScrapBookingDoc && e.ItemType == eItemType.None);
        }

        /// <summary>
        /// 選擇的太陽光電板序號有自行編號
        /// </summary>
        /// <param name="SPlist"></param>
        /// <returns></returns>
        private bool SPlistNotHasno(List<UserSpInfo> UspList)
        {
            var CheckSum = _context.UserSpInfo.Where(x => x.Uid == User.GetUid() && x.Hasno == "0");
            foreach (UserSpInfo usp in UspList)
            {
                if (CheckSum.Any(y => y.Pid == usp.Pid)) return true;
            }
            return false;
        }

        /// <summary>
        /// 選擇的太陽光電板序號有重複序號
        /// </summary>
        /// <param name="SPlist"></param>
        /// <returns></returns>
        private bool SameSPlistNo(List<UserSpInfo> UspList)
        {
            var _Reuslt = _context.UserSpInfo.Where(n =>
                n.Hasno == "1" &&
                n.Sbid != UspList[0].Sbid &&
                UspList.Select(y => y.Sno).Contains(n.Sno)
            ).Any();

            return _Reuslt;
        }

        /// <summary>
        /// 計算先前已審查確認數量(片)
        /// </summary>
        /// <param name="_Pid"></param>
        /// <returns></returns>
        public int getReviewQty(int _Pid)
        {
            var result = _context.UserPvInfo.Where(x => x.Pid == _Pid).Sum(x => x.SpQty);
            return result;
        }
    }

    public class ViewSBmodel
    {
        public ScrapBooking Rec { get; set; }
        public Dictionary<eItemType, Pvis.Biz.ViewModels.AttachmentViewModel> Attachment { internal get; set; }
    }

    public class UserStoAddrOptions
    {
        public int Pid { get; set; }
        public string Storeaddr { get; set; }
    }
    public class SpOptions
    {
        public int Pid { get; set; }
        public string Pvno { get; set; }
    }
    public class UserInfo
    {
        public string Contact { get; set; }
        public string Tel { get; set; }
        public string Email { get; set; }
    }
}


