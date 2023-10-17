using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Pvis.Biz.Models;
using Pvis.Biz.Services;

namespace Pvis.Web.Pages
{
    public class NewsItemModel : PageModel
    {
        private readonly DataDbContext _context;
        public IHttpContextAccessor _httpContextAccessor;

        public NewsItemModel(DataDbContext context, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
        }

        public NewsFrontend News { get; set; }

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            ViewData["id"] = id;
            if (id == null)
            {
                return NotFound();
            }
            var NewsBiz = new NewsBusinessLayer(_context);
            News = (await NewsBiz.GetListForFrontendAsync(id)).FirstOrDefault();

            if (News == null)
            {
                return NotFound();
            }
            return Page();
        }
    }
}
