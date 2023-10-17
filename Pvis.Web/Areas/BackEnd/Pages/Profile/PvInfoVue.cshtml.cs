using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Pvis.Biz.Models;

namespace Pvis.Web.Areas.BackEnd.Pages.Profile
{
    public class PvInfoVueVueModel : PageModel
    {
        private DataDbContext _context;
        public PvInfoVueVueModel(DataDbContext context)
        {
            this._context = context;
        }
        public List<string> Towns { get; set; }
        public async Task OnGet()
        {
            Towns = await _context.Town.Select(x => x.CountyName + x.TownName).ToListAsync();
        }
    }
}
