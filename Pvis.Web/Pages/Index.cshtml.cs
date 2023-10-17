using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Pvis.Biz.Models;
using Pvis.Biz.Services;

namespace Pvis.Web.Pages
{
    public class IndexModel : PageModel
    {
        private readonly DataDbContext _context;

        public IndexModel(DataDbContext context)
        {
            _context = context;
        }

        public IList<NewsFrontend> News { get; set; } = new List<NewsFrontend>();

        public async Task OnGetAsync()
        {
            var NewsBiz = new NewsBusinessLayer(_context);
            News = await NewsBiz.GetListForFrontendAsync(null,5);
        }
    }
}
