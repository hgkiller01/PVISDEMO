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
using Pvis.Biz.Extension;

namespace Pvis.Web.Controller
{
    [Route("api/[controller]")]
    [ApiController]
    [PvisAuthorize(RoleList.Admin, RoleList.Epa, RoleList.Store, RoleList.Teart)]
    public class AuditOperationController : ControllerBase
    {
        private readonly DataDbContext _context;

        public AuditOperationController(DataDbContext context)
        {
            _context = context;
        }

        [HttpPost]
        [Route("Load")]
        public async Task<ActionResult<IEnumerable<ScheduleAudit>>> Load()
        {

            var query = _context.ScheduleAudit.Where(w => w.Aud_State == "Y1").Select(x => x);
            if (!User.HasRole(RoleList.Admin, RoleList.Epa))
            {
               query = query.Where(x => x.Uid == User.GetUid());
            }
            return await query.ToListAsync();
        }

        [HttpPost]
        [Route("GetList")]
        public async Task<IEnumerable<ScheduleAudit>> GetList(SchQry Qry)
        {
            var query = _context.ScheduleAudit.Where(x => x.Aud_State == "Y1");
            var pred = PredicateBuilder.New<ScheduleAudit>(true);

            if (Qry.StartDt.HasValue) pred.And(x => x.Pre_Date >= Qry.StartDt.Value);

            if (Qry.EndDt.HasValue) pred.And(x => x.Pre_Date <= Qry.EndDt.Value);

            if (!String.IsNullOrWhiteSpace(Qry.Aud_State)) pred.And(x => x.Check_State == Qry.Aud_State);
            if (!User.HasRole(RoleList.Admin, RoleList.Epa))
            {
                pred.And(x => x.Uid == User.GetUid());
            }
            return await query.Where(pred).ToListAsync();
        }
        [HttpPost]
        [Route("CheckChange")]
        public async Task<IActionResult> CheckChange(GetPid pid)
        {
            ScheduleAudit audit = _context.ScheduleAudit.Find(pid.Pid);
            audit.Aud_Su_Date = DateTime.Now;
            audit.Check_State = "S";
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
        }
    }
}
