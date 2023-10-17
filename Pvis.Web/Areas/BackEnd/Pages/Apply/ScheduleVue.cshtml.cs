using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Pvis.Biz.Models;
using Pvis.Web.Helper;
using Pvis.Biz.CommEnum;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Hosting;

namespace Pvis.Web.Areas.BackEnd.Pages.Apply
{
    public class ScheduleVue : PageModel
    {
        private DataDbContext _context;
        private IWebHostEnvironment Environment;
        public ScheduleVue(DataDbContext context, IWebHostEnvironment environment)
        {
            Environment = environment;
            _context = context;
        }
        public List<string> Cles { get; set; }
        public List<string> Tres { get; set; }
        public void OnGet()
        {
            string WebRootPath = Environment.WebRootPath;
            string JSON = System.IO.File.ReadAllText(WebRootPath + "/mgr/organclean.json");
            var result = JsonConvert.DeserializeObject<Rootobject>(JSON);
            Cles = result.data.Select(x => x.GetValue(1) as string).ToList();
            Tres = AuthHelper.GetUserListByType(RoleList.Teart, RoleList.Store).Select(x => x.CompanyName).ToList();
        }
    }

    public class Rootobject
    {
        public List<string[]> data { get; set; }
    }

}
