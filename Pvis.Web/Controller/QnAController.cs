using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Pvis.Biz.CommEnum;
using Pvis.Biz.Member;
using Pvis.Web.Helper;

namespace Pvis.Web.Controller
{
    [Route("api/[controller]")]
    [ApiController]
    [PvisAuthorize(RoleList.Admin,RoleList.Epa)]
    public class QnAController : ControllerBase
    {
        [HttpPost]
        [Route("Save")]
        public ActionResult Save(QnA Rec)
        {
            var IsSuccess = Rec.Save(out var Errors);

            if (Errors.Any()) return BadRequest(new { Errors });

            return Ok(new { IsSuccess });
        }
    }
}
