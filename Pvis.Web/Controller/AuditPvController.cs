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
    public class AuditPvController : ControllerBase
    {
        private readonly DataDbContext _context;

        public AuditPvController(DataDbContext context)
        {
            _context = context;
        }

        [HttpPost]
        [Route("GetList")]
        public async Task<ActionResult<IEnumerable<AuditPv>>> GetList(ApQry Qry)
        {
            var pred = PredicateBuilder.New<AuditPv>(true);
            if (!String.IsNullOrWhiteSpace(Qry.PVNo))
            {
                Qry.PVNo = Qry.PVNo.Trim();
                pred = pred.And(x => x.P_PVNo.Contains(Qry.PVNo));
            }

            if (!String.IsNullOrWhiteSpace(Qry.Applicant))
            {
                Qry.Applicant = Qry.Applicant.Trim();
                pred = pred.And(x => x.P_Applicant.Contains(Qry.Applicant));
            }

            if (!String.IsNullOrWhiteSpace(Qry.AuditResult))
            {
                if (Qry.AuditResult == "1")
                    pred = pred.And(x => x.P_PVNo == x.U_pvno && x.P_Applicant == x.U_CompanyName && x.P_PVAddr.Replace("台", "臺") == x.U_pvaddr && x.P_SpQty == x.U_SpQty.ToString());
                else
                    pred = pred.And(x => x.P_PVNo != x.U_pvno || x.P_Applicant != x.U_CompanyName || x.P_PVAddr.Replace("台", "臺") != x.U_pvaddr || x.P_SpQty != x.U_SpQty.ToString());
            }

            return await _context.AuditPv.Where(pred).ToListAsync();
        }

        [HttpPost]
        [Route("Load")]
        public async Task<ActionResult<IEnumerable<AuditPv>>> Load()
        {
            return await _context.AuditPv.ToListAsync();
        }

        public class ApQry
        {
            public string PVNo { get; set; }
            public string Applicant { get; set; }
            public string AuditResult { get; set; }
        }
    }
}
