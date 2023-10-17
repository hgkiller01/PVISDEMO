using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Pvis.Biz.EmailSenderServices;
using Pvis.Biz.Member;
using Pvis.Biz.Extension;
using Pvis.Biz.Services;
using Pvis.Web.Helper;
using Pvis.Biz.Models;
using Pvis.Biz.CommEnum;
using Microsoft.AspNetCore.Http;

namespace Pvis.Web.Controller
{
    [Route("api/[controller]")]
    [ApiController]
    [ValidateAntiForgeryToken]
    [Authorize]
    public class UserAuthController : ControllerBase
    {
        private readonly UserManager<MyAppUser> _userManager;
        private readonly SignInManager<MyAppUser> _signInManager;
        private readonly ApplicationDbContext _application;
        private readonly IEmailSender _emailSender;
        private readonly ILogger<UserAuthController> _logger;
        private readonly DataDbContext _context;

        public UserAuthController(
            UserManager<MyAppUser> userManager,
            SignInManager<MyAppUser> signInManager,
            ApplicationDbContext application,
            IEmailSender emailSender ,
            ILogger<UserAuthController> logger,
            DataDbContext context)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _application = application;
            _emailSender = emailSender;
            _logger = logger;
            _context = context;
        }

        /// <summary>
        /// 登入驗證
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> Post(UserAuth value)
        {
            List<string> errors = new List<string>();

            if (String.IsNullOrWhiteSpace(value.UserId)) errors.Add("帳號未輸入");

            if (String.IsNullOrWhiteSpace(value.UserPwd)) errors.Add("密碼未輸入");

            if (Captcha.ValidateCaptchaCode(value.CaptchaCode, HttpContext) == false) errors.Add("圖形驗證碼錯誤");

            var FailCount = AuthHelper.GetFailCount(HttpContext);

            if (AuthHelper.CheckOverFailCount(HttpContext)) errors.Add("超出連續錯誤次數上限，系統暫時停止登入功能");

            if (errors.Any()) return BadRequest(new { errors , IsOverFailCount = FailCount >= 5 } );

            var result = await _signInManager.PasswordSignInAsync(value.UserId, value.UserPwd, value.RememberMe, lockoutOnFailure: true);
            
            if (result.Succeeded)
            {
                _logger.LogInformation("User logged in.");
                
                var userData = await _userManager.FindByNameAsync(value.UserId);
                AuthHelper.ClearCache(User);
                AuthHelper.ClearFailCount(HttpContext);

                LoginLog log = new LoginLog()
                {
                    ActionTime = DateTime.Now,
                    ActionType = "Login",
                    CompanyName = userData.CompanyName,
                    DisplayName = userData.CompanyName,
                    Uid = userData.Uid,
                    UserName = userData.UserName,
                    IP = Request.HttpContext.Connection.RemoteIpAddress.ToString()
                };
                _application.LoginLog.Add(log);
                _application.SaveChanges();
                if(userData.UserName != "sysadmin")
                {
                    var PasswordData = _application.ChangePwdHistory.Where(x => x.Id == userData.Id);
                    if (PasswordData.Count() <= 0)
                    {
                        HttpContext.Session.SetString("firstpassword", "新帳號尚未修改密碼 若已經修改密碼請先登出後再次登入");
                    }
                    else
                    {
                        DateTime LastLogDate = PasswordData.OrderByDescending(x => x.LogDt).
                              Where(x => x.Id == PasswordData.FirstOrDefault().Id).FirstOrDefault().LogDt;
                        if (LastLogDate.AddMonths(3) <= DateTime.Now)
                        {
                            HttpContext.Session.
                                SetString("password", "您的密碼已逾時三個月未變更 為維護密碼之機密性 建議您立即變行變更");
                        }
                        else
                        {
                            HttpContext.Session.Remove("password");
                            HttpContext.Session.Remove("firstpassword");
                        }
                    }
                }

                return Ok(new
                {
                    IsSuccess = true,
                    Url = Url.Content("~/BackEnd/")
                });
            }

            AuthHelper.LogFailCount(HttpContext);

            if (result.IsLockedOut) errors.Add("帳號已鎖定");

            if (!errors.Any()) errors.Add("登入驗證失敗");

            return BadRequest(new { errors });
        }

        /// <summary>
        /// 產生密碼重設連結
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("ForgotPassword")]
        [AllowAnonymous]
        public async Task<IActionResult> ForgotPassword(UserAuth value)
        {

            List<string> errors = new List<string>();

            if (String.IsNullOrWhiteSpace(value.UserId)) errors.Add("帳號未輸入");

            if (Captcha.ValidateCaptchaCode(value.CaptchaCode, HttpContext) == false) errors.Add("圖形驗證碼錯誤");

            if (errors.Any()) return BadRequest(new { errors });

            var user = await _userManager.FindByNameAsync(value.UserId);

            if (user == null)
            {
                errors.Add("重設密碼連結產生失敗");
                return BadRequest(new { errors });
            }

            var code = (await _userManager.GeneratePasswordResetTokenAsync(user)).Encode();

            var callbackUrl = Url.Page(
                "/ResetPassword", 
                pageHandler:null,
                values: new { area = "BackEnd" , code } , 
                protocol: Request.Scheme );

            string MailBody = $@"
<div style=""font-family: 'Microsoft JhengHei', sans-serif;color: #555;font-size: 12pt;"">
{user.DisplayName} 您好，<br/><br/>
我們已收到你的 {SysConfig.Cfg.SiteName} 密碼重設要求。<br/>
請點透過下列連結重新設定您的密碼。<br/>
<a href=""{callbackUrl}"">更改密碼<a>
</div>
";
            await _emailSender.SendEmailAsync(
                new List<string> { user.Email },
                null,
                SysConfig.Cfg.MailBcc,
                SysConfig.Cfg.SiteName + "重設密碼通知信" ,
                MailBody );

            List<char> _mail = new List<char>();

            var i = 0;
            foreach (var c in user.Email) {
                i++;
                if (i <= 4 || user.Email.Length - i <= 5 ||  c == '.' || c == '@' ) {
                    _mail.Add(c);
                }
                else {
                    _mail.Add('*');
                }
            }

            return Ok( new {
                url = Url.Content("~/BackEnd/") ,
                mail = String.Join("", _mail)
            });

        }


        /// <summary>
        /// 一般使用者重設密碼處理流程
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("ResetPassword")]
        [AllowAnonymous]
        public async Task<IActionResult> ResetPassword(UserAuth value)
        {

            var errors = new List<string>();

            var token = value.code.Decode(Exp: TimeSpan.FromHours(24));

            if (String.IsNullOrWhiteSpace(value.NewPwd)) errors.Add("新密碼未輸入");

            if (String.IsNullOrWhiteSpace(value.ConfirmPwd)) errors.Add("確認密碼未輸入");

            if (String.IsNullOrWhiteSpace(token)) errors.Add("失效的重設密碼連結");

            if (value.ConfirmPwd != value.NewPwd) errors.Add("密碼不一致");

            if (Captcha.ValidateCaptchaCode(value.CaptchaCode, HttpContext) == false) errors.Add("圖形驗證碼錯誤");

            if (errors.Any()) return BadRequest(new { errors });

            var user = await _userManager.FindByNameAsync(value.UserId);

            if (user == null) errors.Add("無效登入帳號");

            if (errors.Any()) return BadRequest(new { errors });

            var Sha256Pwd = value.NewPwd.ToSha256String(user.Id);

            if (_application.ChangePwdHistory.Where(x => x.Id == user.Id).OrderByDescending(x => x.LogDt).Take(3).Where(x => x.PasswordHash == Sha256Pwd).Any())
            {
                errors.Add("密碼與前三次相同");
                return BadRequest(new { errors });
            }

            var _ResutResult = await _userManager.ResetPasswordAsync(user, token, value.NewPwd);

            if (_ResutResult.Succeeded) {

                _application.ChangePwdHistory.Add(new ChangePwdHistory()
                {
                    Id = user.Id,
                    PasswordHash = Sha256Pwd,
                    LogDt = DateTime.Now
                }).State = EntityState.Added;

                await _application.SaveChangesAsync();

                return Ok(new { url = Url.Content("~/BackEnd/Login") });
            }

            errors.AddRange(_ResutResult.Errors.Select(x => x.Description).ToList());

            return BadRequest(new { errors });

        }

        /// <summary>
        /// 一般使用者變更個人密碼
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("ChangePassword")]
        public async Task<IActionResult> ChangePassword(UserAuth value)
        {
            var user = await _userManager.GetUserAsync(User);
            var errors = new List<string>();
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            if (String.IsNullOrWhiteSpace(value.UserPwd) || String.IsNullOrWhiteSpace(value.NewPwd) || String.IsNullOrWhiteSpace(value.ConfirmPwd))
            {
                errors.Add("輸入資料不完全!");
                return BadRequest(new { errors });
            }

            if (value.ConfirmPwd != value.NewPwd)
            {
                errors.Add("密碼輸入不一致!");
                return BadRequest(new { errors });
            }

            var Sha256Pwd = value.NewPwd.ToSha256String(user.Id);

            if (_application.ChangePwdHistory.Where(x => x.Id == user.Id).OrderByDescending(x => x.LogDt).Take(3).Where(x => x.PasswordHash == Sha256Pwd).Any())
            {
                errors.Add("密碼與前三次相同");
                return BadRequest(new { errors });
            }

            var changePasswordResult = await _userManager.ChangePasswordAsync(user, value.UserPwd, value.NewPwd);

            if (changePasswordResult.Succeeded)
            {
                await _signInManager.RefreshSignInAsync(user);

                _application.ChangePwdHistory.Add(new ChangePwdHistory()
                {
                    Id = user.Id,
                    PasswordHash = Sha256Pwd ,
                    LogDt = DateTime.Now
                }).State = EntityState.Added;

                await _application.SaveChangesAsync();

                return Ok();
            }

            errors.AddRange(changePasswordResult.Errors.Select(x => x.Description).ToList());

            return BadRequest(new { errors });
        }

        [HttpPost]
        [Route("ChangeProfile")]
        public async Task<IActionResult> ChangeProfile(MyAppUserFormProfile Rec)
        {
            var user = await _userManager.GetUserAsync(User);
            var sameUsers = _userManager.Users.Where(x => x.CompanyName == Rec.CompanyName && x.CompanyName != user.CompanyName).ToList();
            if (Rec.Id != user.Id) return BadRequest(new { errors = new List<string> { "錯誤資料對應" } });
            if(sameUsers.Count() <= 0)
            {
                user = MemberBusinessLayer.mapper.Map<MyAppUserFormProfile, MyAppUser>(Rec, user);

                var _result = await _userManager.UpdateAsync(user);

                AuthHelper.ClearCache(User);

                await _signInManager.RefreshSignInAsync(user);
            }
            else
            {
                return BadRequest(new { errors = new List<string> { "此公司名稱重復" } });
            }


            return Ok();

        }

        public class UserAuth
        {
            public string UserId { get; set; }
            public string UserPwd { get; set; }
            public Boolean RememberMe { get; set; }
            public string NewPwd { get; set; }
            public string ConfirmPwd { get; set; }

            public string CaptchaCode { get; set; }

            public string code { get; set; }
        }
    }
}
