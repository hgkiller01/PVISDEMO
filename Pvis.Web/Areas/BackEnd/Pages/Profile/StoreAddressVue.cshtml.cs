using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Pvis.Biz.Member;
using Pvis.Biz.Models;
using Pvis.Biz.Services;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;


namespace Pvis.Web.Areas.BackEnd.Pages.Profile
{
    public class StoreAddressVueModel : PageModel
    {
        private DataDbContext _context;
        public StoreAddressVueModel(DataDbContext context)
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
