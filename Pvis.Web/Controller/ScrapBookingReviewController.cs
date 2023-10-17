using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MimeKit;
using Pvis.Biz.CommEnum;
using Pvis.Biz.EmailSenderServices;
using Pvis.Biz.Extension;
using Pvis.Biz.Member;
using Pvis.Biz.Models;
using Pvis.Web.Helper;

namespace Pvis.Web.Controller
{
    [Route("api/[controller]")]
    [ApiController]
    [ValidateAntiForgeryToken]
    [PvisAuthorize(RoleList.Admin, RoleList.Auditor)]
    public class ScrapBookingReviewController : ControllerBase
    {
        private readonly DataDbContext _context;
        private readonly IEmailSender _email;
        private static IMapper mapper;

        public ScrapBookingReviewController(DataDbContext context, IEmailSender email)
        {
            _context = context;
            _email = email;

            var configuration = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<UserPvInfo, ApplyPvInfo>();
            });
            mapper = configuration.CreateMapper();
        }

        [HttpPost]
        [Route("Save")]
        public async Task<IActionResult> Save(ScrapBookingReview SBR)
        {
            #region 表單驗證
            var errors = new List<string>();

            if (SBR.Status == "M" || SBR.Status == "N")
            {
                if (string.IsNullOrWhiteSpace(SBR.Desc))
                {
                    errors.Add("『意見』未填寫");
                }
            }

            if (errors.Count > 0)
            {
                return BadRequest(new { IsSuccess = false, errors });
            }
            #endregion

            var State = SBR.Pid > 0 ? EntityState.Modified : EntityState.Added;

            if (State == EntityState.Added)
            {
                SBR.CKDate = DateTime.Now;
                SBR.CKUid = User.GetUid();
                SBR.CKName = User.GetDisplayName();
            }
            var SB = await _context.ScrapBooking.FindAsync(SBR.SBPid);
            SB.Status = SBR.Status;
            SB.SBCity = (await _context.UserStoreAddress.FindAsync(SB.Uspid)).Storeaddr.Substring(0, 3);
            SB.SP = await _context.UserSpInfo.Where(x => x.Sbid == SBR.SBPid).ToListAsync();
            SB.Qty = SB.SP.Distinct().Count();

            using (var transaction = _context.Database.BeginTransaction())
            {
                _context.Entry(SBR).State = State;
                await _context.SaveChangesAsync();

                #region 更新案場排出登記表
                _context.Add(SB).State = EntityState.Modified;
                await _context.SaveChangesAsync();
                #endregion

                #region 通過狀態，另存設備登記資料
                if (SB.Status == "Y1" || SB.Status == "Y2")
                {
                    var ListUSI = await _context.UserSpInfo.Where(x => x.Sbid == SB.Pid).ToListAsync();
                    var pvid = ListUSI.Select(x => x.Pvid).Distinct();
                    var ListUPI = await _context.UserPvInfo.Where(x => pvid.Contains(x.Pid)).ToListAsync();
                    foreach (var UPI in ListUPI)
                    {
                        var API = mapper.Map<ApplyPvInfo>(UPI);
                        API.SBPid = SB.Pid;

                        _context.Entry(API).State = EntityState.Added;
                        await _context.SaveChangesAsync();
                    }
                }
                #endregion

                transaction.Commit();
            }

            await SendMail(SB, SBR);

            return Ok(new { Rec = SB });
        }

        /// <summary>寄送郵件</summary>
        /// <param name="SB">案場排出登記表</param>
        /// <param name="SBR">審查確認</param>
        private async Task SendMail(ScrapBooking SB, ScrapBookingReview SBR)
        {
            var message = $@"{SB.Contact} 先生/小姐，您好<br><br>
申請編號：{SB.Bookingno}
<p style='color:red'>";

            switch (SB.Status)
            {
                case "Y1":
                    message += $"確認結果為【通過】";
                    break;
                case "M":
                    message += $"確認結果為【補正】，原因：{SBR.Desc}";
                    break;
                case "N":
                    message += $"確認結果為【不通過】，原因：{SBR.Desc}";
                    break;
            }
            message += $@"</p>
詳細內容，請至{SysConfig.Cfg.SiteName}查詢，感謝您<br>
【此郵件為系統自動發送，請勿回信！】";

            await _email.SendEmailAsync(
                new List<string> { SB.Email },
                SysConfig.Cfg.ReviewCcEmail.Union(SysConfig.Cfg.MailBcc),
                null,
                $"{SB.Bookingno}廢太陽能光電板案場排出登記確認通知信",
                message
                );
        }
    }
}


