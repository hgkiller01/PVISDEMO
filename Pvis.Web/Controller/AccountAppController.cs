using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Pvis.Biz.CommEnum;
using Pvis.Biz.EmailSenderServices;
using Pvis.Biz.Extension;
using Pvis.Biz.Member;
using Pvis.Biz.Models;
using Pvis.Biz.Services;
using Pvis.Web.Helper;

namespace Pvis.Web.Controller
{
    [Route("api/[controller]")]
    [ApiController]
    [PvisAuthorize(RoleList.Admin,RoleList.Epa)]
    [ValidateAntiForgeryToken]
    public class AccountAppController : ControllerBase
    {

        private readonly AccountAppBusinessLayer _AccountAppBiz;
        private readonly MemberBusinessLayer _MemberBiz;
        private readonly DataDbContext _context;
        private readonly IEmailSender _emailSender;

        public AccountAppController(
            DataDbContext context,
            ApplicationDbContext appcontext,
            UserManager<MyAppUser> userManager,
            RoleManager<IdentityRole> roleManager,
            IEmailSender emailSender,
            IHttpContextAccessor httpContextAccessor)
        {
            _AccountAppBiz = new AccountAppBusinessLayer(
                context,
                appcontext,
                httpContextAccessor);

            _MemberBiz = new MemberBusinessLayer(
                userManager,
                roleManager,
                context,
                appcontext);
            _context = context;
            _emailSender = emailSender;

        }

        /// <summary>
        /// 帳號申請
        /// </summary>
        /// <param name="Rec"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("Apply")]
        [AllowAnonymous]
        public async Task<ActionResult> Apply(AccountAppViewModel Rec)
        {
            List<string> errors = new List<string>();

            if (Captcha.ValidateCaptchaCode(Rec.CaptchaCode, HttpContext) == false)
            {
                errors.Add("圖形驗證碼輸入錯誤");
                return BadRequest(new { errors });
            }

            errors = await _AccountAppBiz.Apply(Rec, Request.HttpContext.Connection.RemoteIpAddress.ToString());

            if (errors.Any())
            {
                return BadRequest(new { errors });
            }

            await _emailSender.SendEmailAsync(
                new List<string> { Rec.Email },
                null,
                SysConfig.Cfg.MailBcc,
                SysConfig.Cfg.SiteName + "帳號申請通知信",
                Rec.GetNofityMailBoyForApply(SysConfig.Cfg.SiteName));

            return Ok(new { url = Url.Content("~/") });
        }

        /// <summary>
        /// 審查存檔
        /// </summary>
        /// <param name="Rec"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("SendReview")]
        public async Task<ActionResult<AccountAppViewModel>> SendReview(AccountAppViewModel Rec)
        {

            var errors = await _AccountAppBiz.SendReview(Rec);

            if (errors?.Any() == true) return BadRequest(new { errors });

            if (Rec.Status == AppStatusList.accept)
            {
                var User = new MyAppUserForm()
                {
                    Address = Rec.Address,
                    UserName = Rec.ControlNo,
                    Email = Rec.Email,
                    DisplayName = Rec.UserName,
                    CompanyName = Rec.UserRole == AppRoleList.AppPersonal ? Rec.UserName : Rec.CompanyName,
                    PhoneNumber = Rec.Tel,
                    AppPid = Rec.Pid,
                    Roles = Rec.GetDefualtRoles(),
                    CaseName = Rec.CaseName,
                    CaseEmail = Rec.CaseEmail
                };
                User = await _MemberBiz.SaveAsync(User);
                if (User.errors?.Any() == true)
                {
                    User.errors.Insert(0, "審查通過帳號開通發生異常");
                    return BadRequest(new { User.errors });
                }
                await _emailSender.SendEmailAsync(
                    new List<string> { Rec.Email },
                    null,
                    SysConfig.Cfg.MailBcc,
                    SysConfig.Cfg.SiteName + "帳號核可通知信",
                    Rec.GetNofityMailBoy(User.NewPassWord, SysConfig.Cfg.SiteName));
            }

            if (Rec.Status == AppStatusList.reject)
            {
                await _emailSender.SendEmailAsync(
                    new List<string> { Rec.Email },
                    null,
                    SysConfig.Cfg.MailBcc,
                    SysConfig.Cfg.SiteName + "帳號審查退件通知信",
                    Rec.GetRejectMailBoy(SysConfig.Cfg.SiteName));
            }

            return Rec;
        }

        /// <summary>
        /// 清單查詢
        /// </summary>
        /// <param name="Qry"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("GetList")]
        public async Task<IEnumerable<AccountAppViewModel>> GetList(AccountAppQry Qry)
        {
            return await _AccountAppBiz.GetList(Qry);
        }

        /// <summary>
        /// 單筆資料取得
        /// </summary>
        /// <param name="Qry"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("GetItem")]
        public async Task<ActionResult<AccountAppViewModel>> GetItem(AccountAppQry Qry)
        {
            var Rec = await _AccountAppBiz.GetItem(Qry);
            Rec.AttList = (await Rec.GetAttListAsync(_context)).Select(x => new
            {
                DisplayName = EnumHelper<eItemType>.GetDisplayValue(x.ItemType) ,
                url = Url.Content(x.FilePath),
                x.OriginalFileName
            }).ToList<object>();
            if (Rec != null) return Rec;
            return BadRequest(new { errors = new List<string>() { "資料取得失敗" } });
        }

    }
}
