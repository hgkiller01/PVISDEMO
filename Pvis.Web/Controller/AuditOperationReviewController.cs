using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LinqKit;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Pvis.Biz.CommEnum;
using Pvis.Biz.Extension;
using Pvis.Biz.Member;
using Pvis.Biz.Models;

namespace Pvis.Web.Controller
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    [PvisAuthorize(RoleList.Admin, RoleList.Epa, RoleList.Auditor)]
    public class AuditOperationReviewController : ControllerBase
    {
        private readonly DataDbContext _context;

        public AuditOperationReviewController(DataDbContext context)
        {
            _context = context;
        }

        [HttpPost]
        public async Task<ActionResult<IEnumerable<ScheduleAudit>>> Load()
        {

            var query = _context.ScheduleAudit.Where(x => x.Check_State == "S" || x.Check_State == "M" || x.Check_State == "Y1");
            query = _context.ScheduleAudit.Where(x => x.Aud_State == "Y1" && x.Check_State != "1");
            return await query.ToListAsync();
        }

        [HttpPost]
        public async Task<ActionResult<IEnumerable<ScheduleAudit>>> GetList(SchQry Qry)
        {
            var query = _context.ScheduleAudit.Where(x => x.Check_State == "S" || x.Check_State == "M" || x.Check_State == "Y1");
            query = _context.ScheduleAudit.Where(x => x.Aud_State == "Y1" && x.Check_State != "1");
            var pred = PredicateBuilder.New<ScheduleAudit>(true);

            if (Qry.StartDt.HasValue) pred.And(x => x.Pre_Date >= Qry.StartDt.Value);

            if (Qry.EndDt.HasValue) pred.And(x => x.Pre_Date <= Qry.EndDt.Value);
            if (!string.IsNullOrWhiteSpace(Qry.CompanyName)) pred.And(x => x.CompanyName.Contains(Qry.CompanyName));
            if (!String.IsNullOrWhiteSpace(Qry.Aud_State)) pred.And(x => x.Check_State == Qry.Aud_State);
            var temp = query.ToList();
            return await query.Where(pred).ToListAsync();
        }
        [HttpPost]
        public async Task<IActionResult> CheckChange(ScheduleAudit audit)
        {
            ScheduleAudit Modiyaudit = _context.ScheduleAudit.Find(audit.Pid);
            Modiyaudit.Aud_Su_Date = DateTime.Now;
            Modiyaudit.Check_State = audit.Check_State;
            Modiyaudit.Aud_Su_opinion = audit.Aud_Su_opinion;
            Modiyaudit.Aud_Desc = audit.Aud_Desc;
            await _context.SaveChangesAsync();
            return Ok();
        }
        public class GetPid
        {
            public int Pid { get; set; }
        }
        public class ViewAOmodel
        {
            public string Aud_Sch_No { get; set; }
            public DateTime? Pre_Date { get; set; }
            public DateTime? Tre_Pre_Date { get; set; }
            public int Uid { get; set; }
        }

        public class SchQry
        {
            public int Pid { get; set; }
            public string City { get; set; }
            public int Uspid { get; set; }
            public DateTime? StartDt { get; set; }
            public DateTime? EndDt { get; set; }
            public string Aud_Sch_No { get; set; }
            public string Bookingno { get; set; }
            public string Aud_State { get; set; }
            public string CompanyName { get; set; }
        }
    }
}
