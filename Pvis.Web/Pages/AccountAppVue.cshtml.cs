using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Pvis.Biz.Models;
using Pvis.Biz.Services;

namespace Pvis.Web.Pages
{
    public class AccountAppVueModel : PageModel
    {
        private DataDbContext _context;

        public AccountAppVueModel(DataDbContext context) {
            this._context = context;
        }

        public List<string> Towns { get; set; }

        public readonly Dictionary<byte, string> AppRoleCode = AccountAppBusinessLayer.AppRoleCode;

        public async Task OnGet()
        {
            Towns = await _context.Town.Select(x => x.CountyName + x.TownName).ToListAsync();
        }
    }
}
