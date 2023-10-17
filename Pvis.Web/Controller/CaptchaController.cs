using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.IO;
using Pvis.Web.Helper;

namespace Pvis.Web.Controller
{
    [Route("api/[controller]")]
    [ApiController]
    public class CaptchaController : ControllerBase
    {
        // GET: api/Captcha/width/height
        [HttpGet("{width}/{height}")]
        public IActionResult Get(int width, int height)
        {
            if (width < 10 || width > 256) return NotFound();
            if (height < 10 || height > 256) return NotFound();
            var result = Captcha.GenerateCaptchaImage(width, height, HttpContext);
            Stream s = new MemoryStream(result.CaptchaByteData);
            return new FileStreamResult(s, "image/png");
        }
        [HttpGet]
        [Route("GetWave")]
        public IActionResult GetWave()
        {
            Stream s = Captcha.GetWave(HttpContext);
            if (s is null)
            {
                return new NotFoundResult();
            }
            return new FileStreamResult(s, "audio/wav");
        }

    }
}
