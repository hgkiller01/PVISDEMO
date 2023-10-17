using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Pvis.Biz.CommEnum;
using Pvis.Biz.EmailSenderServices;
using Pvis.Biz.Member;
using Pvis.Web.Helper;


namespace Pvis.Web.Controller
{
    [Route("api/[controller]")]
    [ApiController]
    [PvisAuthorize(RoleList.Admin)]
    public class SysConfigController : ControllerBase
    {
        private readonly IEmailSender _emailSender;

        public SysConfigController(IEmailSender emailSender)
        {
            _emailSender = emailSender;
        }

        [HttpPost]
        [Route("Load")]
        public ActionResult Load()
        {
            try
            {
                return Ok(SysConfig.Cfg);
            }
            catch {
                var errors = new List<string>() { "設定檔載入發生異常" };
                return BadRequest(new { errors } );
            }
        }

        [HttpPost]
        [Route("Save")]
        public ActionResult Save(SysConfig Rec)
        {
            var IsSuccess = Rec.Save(out List<string> errors);

            if (IsSuccess) _emailSender.ReloadConfig();

            if (errors.Any()) return BadRequest(new { errors });

            return Ok(new
            {
                IsSuccess
            });
        }
    }
}
