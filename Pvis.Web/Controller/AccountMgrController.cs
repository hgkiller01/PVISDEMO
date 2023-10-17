using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Pvis.Biz.CommEnum;
using Pvis.Biz.EmailSenderServices;
using Pvis.Biz.Member;
using Pvis.Biz.Models;
using Pvis.Biz.Services;
using Pvis.Web.Helper;

namespace Pvis.Web.Controller
{
    [Route("api/[controller]")]
    [ApiController]
    [ValidateAntiForgeryToken]
    [PvisAuthorize(RoleList.Admin,RoleList.Epa)]
    public class AccountMgrController : ControllerBase
    {

        private static IMapper mapper;
        static AccountMgrController()
        {
            var configuration = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<MyAppUserForm, MyAppUser>();
                cfg.CreateMap<MyAppUser, MyAppUserForm>();
            });
            mapper = configuration.CreateMapper();
        }

        private readonly ILogger<AccountMgrController> _logger;
        private readonly MemberBusinessLayer _MemberBiz;
        private readonly UserManager<MyAppUser> _userManager;
        private readonly SignInManager<MyAppUser> _signInManager;
        private readonly IEmailSender _IEmailSender;

        public AccountMgrController(
            ApplicationDbContext applicationDbContext,
            DataDbContext context,
            RoleManager<IdentityRole> roleManager,
            UserManager<MyAppUser> userManager,
            SignInManager<MyAppUser> signInManager ,
            IEmailSender emailSender,
            ILogger<AccountMgrController> logger)
        {
            _MemberBiz = new MemberBusinessLayer(
                userManager ,
                roleManager ,
                context ,
                applicationDbContext
            );
            _userManager = userManager;
            _signInManager = signInManager;
            _IEmailSender = emailSender;
            _logger = logger;
        }

        /// <summary>
        /// 清單資料取得
        /// </summary>
        /// <param name="Qry"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("GetList"),ActionName("GetList")]
        public async Task<ActionResult<IEnumerable<MyAppUserForm>>> GetList(MemberQry Qry)
        {
            var Result = (await _MemberBiz.GetListAsync(Qry)).Select(x => mapper.Map<MyAppUserForm>(x));
            return Ok(Result);
        }

        /// <summary>
        ///  單筆資料取得
        /// </summary>
        /// <param name="Rec"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("GetItem")]
        public async Task<ActionResult<MyAppUserForm>> GetItem(MyAppUserForm Rec)
        {
            var User = await _MemberBiz.GetItemAsync(Rec);
            
            if (User == null) return BadRequest(new { errors = new List<string>() { "查無對應使用者資料" } });

            return User;
        }

        /// <summary>
        /// 強制重設使用者密碼.
        /// </summary>
        /// <param name="Rec"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("ResetPassWord")]
        [PvisAuthorize(RoleList.Admin)]
        public async Task<ActionResult<MyAppUserForm>> ResetPassWord(MyAppUserForm Rec)
        {
            var User = await _MemberBiz.ResetPassWordAsync(Rec);
            if (User == null) return BadRequest(new { errors = new List<string>() { "查無對應使用者資料" } });
            return User;
        }

        /// <summary>
        /// 資料存檔
        /// </summary>
        /// <param name="Rec"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("Save")]
        [PvisAuthorize(RoleList.Admin)]
        public async Task<ActionResult<MyAppUserForm>> Save(MyAppUserForm Rec)
        {
            if (string.IsNullOrWhiteSpace(Rec.Id) && Regex.IsMatch(Rec.UserName, "^[A-Z][A-Z0-9][0-9]{6}" , RegexOptions.IgnoreCase ) ) {
                return BadRequest(new { errors = new List<string> { "輸入的登入名稱為保留範圍無法使用" }  });
            }

            var User = await _MemberBiz.SaveAsync(Rec);

            Rec.ClearCache();

            if ( User.errors?.Any() == true ) return BadRequest(new { User.errors });

            await SendCreateNotify(User);

            return User;
        }

        /// <summary>
        /// 發送帳號開通通知信件
        /// </summary>
        /// <param name="user"></param>
        [NonAction]
        private async Task SendCreateNotify(MyAppUserForm user)
        {
            if (String.IsNullOrWhiteSpace(user.Id) || String.IsNullOrWhiteSpace(user.NewPassWord)) return;

            string MailBody = $@"<div style=""font-family: 'Microsoft JhengHei', sans-serif;color: #555;font-size: 12pt;"">
親愛的 {user.DisplayName} 先生/小姐 您好：<br/><br/>
系統管理員已為您開通帳號<br/>
　　帳號： <b style='color:#F66'>{user.UserName}</b><br/>
　　密碼： <b style='color:#F66'>{user.NewPassWord}</b><br/>
提醒您，此密碼為系統自動產生，<span style='color:#F66'>建議儘速更新密碼</span>，請牢記並妥善保管帳號及密碼，謹防他人冒用。<br/>
如欲修改個人資料或密碼，請至　密碼變更/個人資料維護 修改！<br/>
{SysConfig.Cfg.SiteName} 敬啟<br/>
【此郵件為系統自動發送，請勿回信!】
            </ div>";
            await _IEmailSender.SendEmailAsync(
                new List<string> { user.Email },
                null,
                null,
                SysConfig.Cfg.SiteName + "帳號啟用通知信",
                MailBody
                );
        }



        /// <summary>
        /// 切換目前使用者身份
        /// </summary>
        /// <param name="Rec"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("SwitchUser")]
        [PvisAuthorize(RoleList.Admin)]
        public async Task<ActionResult> SwitchUser(MyAppUserForm Rec)
        {
            var user = await _userManager.FindByIdAsync(Rec.Id);
            await _signInManager.RefreshSignInAsync(user);
            return Ok(new
            {
                url = Url.Content("~/BackEnd")
            });
        }

        /// <summary>
        /// 移除使用者
        /// </summary>
        /// <param name="Rec"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("Delete")]
        [PvisAuthorize(RoleList.Admin)]
        public async Task<ActionResult> Delete(MyAppUserForm Rec)
        {
            var Result = await _MemberBiz.DeleteAsync(Rec);
            if (!Result) return BadRequest(new { errors = new List<string>() { "帳號移除失敗" } });
            return Ok();
        }
    }
}
