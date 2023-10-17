using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Pvis.Biz.EmailSenderServices;
using Pvis.Web.Helper;

namespace Pvis.Web.Controller
{
    [Route("api/[controller]")]
    [ApiController]
    public class ContactUsController : ControllerBase
    {

        private readonly IEmailSender _emailSender;

        public ContactUsController(IEmailSender emailSender)
        {
            _emailSender = emailSender;
        }

        /// <summary>
        /// 寄送聯絡我們相關訊息
        /// </summary>
        /// <param name="Rec"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> Post(Msg Rec)
        {
            List<string> errors = new List<string>();

            if (!Captcha.ValidateCaptchaCode(Rec.Captcha, HttpContext)) errors.Add("驗證碼輸入錯誤");

            await Task.Delay(new Random().Next(1500, 2500));

            if (errors.Any()) return BadRequest(new { errors });

            await _emailSender.SendEmailAsync(
                SysConfig.Cfg.ContactUsEmail,
                null,
                SysConfig.Cfg.MailBcc,
                $"{SysConfig.Cfg.SiteName}使用者詢問信件" , 
                Rec.GetMailBody(HttpContext.Connection.RemoteIpAddress) 
            );

            return Ok(Rec);
        }

        public class Msg
        {
            [Required(ErrorMessage = "問題內容未輸入")]
            [MinLength(15, ErrorMessage = "問題內容至少 15 個字")]
            public string Body { get; set; }
            [Required(ErrorMessage = "姓名未輸入")]
            [MinLength(2, ErrorMessage = "姓名過短")]
            public string UserName { get; set; }
            [Phone(ErrorMessage = "電話格式錯誤")]
            [Required(ErrorMessage = "電話未輸入")]
            public string Tel { get; set; }
            [EmailAddress(ErrorMessage = "E-mail格式錯誤")]
            [Required(ErrorMessage = "E-mail未輸入")]
            public string Email { get; set; }
            [Required(ErrorMessage = "主旨未輸入")]
            [MinLength(5, ErrorMessage = "主旨最少 5 個字")]
            public string Subject { get; set; }
            [Required(ErrorMessage = "圖形驗證碼未輸入")]
            public string Captcha { get; set; }

            internal string GetMailBody(System.Net.IPAddress remoteIpAddress)
            {
                StringBuilder _Body = new StringBuilder();
                _Body.Append("<div style=\"font-family: 'Microsoft JhengHei', sans-serif;color: #555;font-size: 12pt;\">");
                _Body.Append("<b style='width: 5em;display:inline-block;'>姓名:</b>" + this.UserName + "<br/>");
                _Body.Append("<b style='width: 5em;display:inline-block;'>電話:</b>" + this.Tel + "<br/>");
                _Body.Append("<b style='width: 5em;display:inline-block;'>填寫時間:</b>" + DateTime.Now.ToString() + "<br/>");
                _Body.Append("<b style='width: 5em;display:inline-block;'>來源IP:</b>" + remoteIpAddress.ToString() + "<br/>");
                _Body.Append("<b style='width: 5em;display:inline-block;'>Email:</b>" + this.Email + "<br/>");
                _Body.Append("<b style='width: 5em;display:inline-block;'>主旨:</b>" + this.Subject + "<br/><hr/>");
                _Body.Append("<pre>" + this.Body + "</pre>");
                _Body.Append("</div>");

                return _Body.ToString();
            }
        }
    }
}
