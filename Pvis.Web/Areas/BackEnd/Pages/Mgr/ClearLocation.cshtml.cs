using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Pvis.Biz.Models;

namespace Pvis.Web.Areas.BackEnd.Pages.Mgr
{
    public class ClearLocationModel : PageModel
    {
        private DataDbContext _context;
        public ClearLocationModel(DataDbContext context)
        {
            _context = context;
        }
        public List<string> Towns { get; set; }
        public async Task OnGet()
        {
            Towns = await _context.Town.Select(x => x.CountyName + x.TownName).ToListAsync();
        }
    }
}
