using System;
using System.Collections.Generic;
using System.Linq;
using LinqKit;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Pvis.Biz.CommEnum;
using Pvis.Biz.Extension;
using Pvis.Biz.Models;
using Pvis.Biz.Member;
using System.IO;
using Pvis.Biz.Services;
using AutoMapper;

namespace Pvis.Web.Controller
{
    [Route("api/[controller]")]
    [ApiController]
    [PvisAuthorize(RoleList.Admin, RoleList.Epa, RoleList.Auditor)]
    public class ApplyPvListController : ControllerBase
    {
        private readonly DataDbContext _context;

        public ApplyPvListController(DataDbContext context)
        {
            _context = context;
        }

        [HttpPost]
        [Route("GetList")]
        public async Task<ActionResult<IEnumerable<ApplyPvList>>> GetList(PVListQry Qry)
        {
            var pred = PredicateBuilder.New<ApplyPvList>(true);
            if (!String.IsNullOrWhiteSpace(Qry.Applicant))
            {
                Qry.Applicant = Qry.Applicant.Trim();
                pred = pred.And(x => x.Applicant.Contains(Qry.Applicant));
            }

            if (!String.IsNullOrWhiteSpace(Qry.City))
            {
                Qry.City = Qry.City.Replace('臺', '台');
                pred = pred.And(x => x.PVAddr.Contains(Qry.City) || x.PVCadastre.Contains(Qry.City));
            }

            if (!String.IsNullOrWhiteSpace(Qry.PVNo))
            {
                Qry.PVNo = Qry.PVNo.Trim();
                pred = pred.And(x => x.PVNo.Contains(Qry.PVNo));
            }

            if (Qry.StartDt.HasValue) pred.And(x => x.FinDate.AddDays(7300) >= Qry.StartDt.Value);

            if (Qry.EndDt.HasValue) pred.And(x => x.FinDate.AddDays(7300) <= Qry.EndDt.Value);

            if (Qry.PETypeID > 0)
            {
                pred = pred.And(x => x.PETypeID == Qry.PETypeID);
            }

            return await _context.ApplyPvList.Where(pred).ToListAsync();
        }

        [HttpPost]
        [Route("Load")]
        public async Task<ActionResult<IEnumerable<ApplyPvList>>> Load()
        {
            return await _context.ApplyPvList.ToListAsync();
        }

        [HttpPost]
        [Route("GetCityOptions")]
        public async Task<ActionResult<List<County>>> GetCityOptions()
        {
            return await _context.County.ToListAsync();
        }

        [HttpPost]
        [Route("GetView")]
        public async Task<ActionResult<IEnumerable<ApplyPvListItem>>> View(PVListQry Qry)
        {
            var pred = PredicateBuilder.New<ApplyPvListItem>(true);
            if (!String.IsNullOrWhiteSpace(Qry.PVNo))
            {
                Qry.PVNo = Qry.PVNo.Trim();
                pred = pred.And(x => x.PVNo.Contains(Qry.PVNo));
            }
            return await _context.ApplyPvListItem.Where(pred).ToListAsync();
        }

        public class PVListQry
        {
            public string Applicant { get; set; }

            public string City { get; set; }

            public string PVNo { get; set; }

            public DateTime? StartDt { get; set; }

            public DateTime? EndDt { get; set; }

            public byte PETypeID { get; set; }
        }
    }
}
