using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LinqKit;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Pvis.Biz.CommEnum;
using Pvis.Biz.Member;
using Pvis.Biz.Models;

namespace Pvis.Web.Controller
{
    [Route("api/[controller]")]
    [ApiController]
    [PvisAuthorize(RoleList.Admin, RoleList.Epa, RoleList.Auditor)]
    public class ModuleFacListController : ControllerBase
    {
        private readonly DataDbContext _context;

        public ModuleFacListController(DataDbContext context)
        {
            _context = context;
        }

        [HttpPost]
        [Route("GetList")]
        public async Task<ActionResult<IEnumerable<ModuleFacList>>> GetList(MFListQry Qry)
        {
            var pred = PredicateBuilder.New<ModuleFacList>(true);
            if (!String.IsNullOrWhiteSpace(Qry.Fac_Name))
            {
                Qry.Fac_Name = Qry.Fac_Name.Trim();
                pred = pred.And(x => x.Fac_Name.Contains(Qry.Fac_Name));
            }

            if (!String.IsNullOrWhiteSpace(Qry.City))
            {
                Qry.City = Qry.City.Trim();
                pred = pred.And(x => x.Company_Addr.Contains(Qry.City) || x.Fac_Addr.Contains(Qry.City));
            }


            return await _context.ModuleFacList.Where(pred).ToListAsync();
        }

        [HttpPost]
        [Route("Load")]
        public async Task<ActionResult<IEnumerable<ModuleFacList>>> Load()
        {
            return await _context.ModuleFacList.ToListAsync();
        }

        [HttpPost]
        [Route("GetCityOptions")]
        public async Task<ActionResult<List<County>>> GetCityOptions()
        {
            return await _context.County.ToListAsync();
        }

        public class MFListQry
        {
            public string Fac_Name { get; set; }

            public string City { get; set; }
        }
    }
}
