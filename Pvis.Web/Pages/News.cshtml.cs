using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Pvis.Biz.Models;
using Pvis.Biz.Services;

namespace Pvis.Web.Pages
{
    public class NewsModel : PageModel
    {
        private readonly DataDbContext _context;

        public NewsModel(DataDbContext context)
        {
            _context = context;
        }

        public IList<NewsFrontend> News { get; set; }

        public async Task OnGetAsync()
        {
            var NewsBiz = new NewsBusinessLayer(_context);
            News = await NewsBiz.GetListForFrontendAsync();
        }
    }
}
