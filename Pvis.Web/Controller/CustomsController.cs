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
    public class CustomsController : ControllerBase
    {
        private readonly DataDbContext _context;

        public CustomsController(DataDbContext context)
        {
            _context = context;
        }

        [HttpPost]
        [Route("GetList")]
        public async Task<ActionResult<IEnumerable<Customs>>> GetList(CtListQry Qry)
        {
            var pred = PredicateBuilder.New<Customs>(true);
            if (!String.IsNullOrWhiteSpace(Qry.Fac_ID))
            {
                Qry.Fac_ID = Qry.Fac_ID.Trim();
                pred = pred.And(x => x.Fac_ID.Contains(Qry.Fac_ID));
            }

            if (!String.IsNullOrWhiteSpace(Qry.Fac_Name))
            {
                Qry.Fac_Name = Qry.Fac_Name.Trim();
                pred = pred.And(x => x.Fac_Name.Contains(Qry.Fac_Name));
            }

            if (Qry.StartDt.HasValue) pred.And(x => x.CustDate >= Qry.StartDt.Value);

            if (Qry.EndDt.HasValue) pred.And(x => x.CustDate <= Qry.EndDt.Value);


            return await _context.Customs.Where(pred).ToListAsync();
        }

        [HttpPost]
        [Route("Load")]
        public async Task<ActionResult<IEnumerable<Customs>>> Load()
        {
            return await _context.Customs.ToListAsync();
        }

        public class CtListQry
        {
            public string Fac_ID { get; set; }
            public string Fac_Name { get; set; }
            public DateTime? StartDt { get; set; }
            public DateTime? EndDt { get; set; }
        }
    }
}
