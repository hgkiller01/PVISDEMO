using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LinqKit;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Pvis.Biz.CommEnum;
using Pvis.Biz.Extension;
using Pvis.Biz.Member;
using Pvis.Biz.Models;
using Pvis.Web.Helper;


namespace Pvis.Web.Controller
{
    [Route("api/[controller]")]
    [ApiController]
    [PvisAuthorize(RoleList.Admin, RoleList.Epa, RoleList.Store, RoleList.Teart)]
    public class SchedulePayBackController : ControllerBase
    {
        private readonly DataDbContext _context;

        public SchedulePayBackController(DataDbContext context)
        {
            _context = context;
        }

        [HttpPost]
        [Route("Load")]
        public async Task<ActionResult<IEnumerable<ScheduleController.ViewSRmodel>>> Load()
        {
            var query = from a in _context.Set<Schedule>() 
                        where a.Cle_State == "D1" || a.Cle_State == "C1"
                        join b in _context.Set<Schedule_SB>()
                            on a.Cle_Sch_No equals b.Cle_Sch_No
                        join c in _context.Set<ScrapBooking>()
                            on b.Bookingno equals c.Bookingno
                        join d in _context.Set<UserSpInfo>()
                            on c.Pid equals d.Sbid
                        group  a by new { a.Pid, a.Cle_Sch_No, a.Cle_Date, a.Cle_Name, a.Tre_Name, a.Cle_State } into g
                        select new ScheduleController.ViewSRmodel
                        {
                            Pid = g.Key.Pid,
                            Cle_Sch_No = g.Key.Cle_Sch_No,
                            Cle_Date = g.Key.Cle_Date,
                            Cle_Name = g.Key.Cle_Name,
                            Tre_Name = g.Key.Tre_Name,
                            Cle_State = g.Key.Cle_State,
                            Cle_Qty = g.Count()
                        };
            var result = query.Where(x => x.Tre_Name == User.GetCompanyName()).ToList();
            var pred = PredicateBuilder.New<ScheduleController.ViewSRmodel>(true);
            ScheduleController.SchQry Qry = new ScheduleController.SchQry();
            //Qry.Cle_State = "D1";
            //if (!String.IsNullOrWhiteSpace(Qry.Cle_State)) pred.And(x => x.Cle_State == Qry.Cle_State);
            if (!User.HasRole(RoleList.Admin, RoleList.Epa))
            {
                query = query.Where(x => x.Tre_Name == User.GetCompanyName());
            }
            return await query.Where(pred).ToListAsync();
        }

        [HttpPost]
        [Route("GetList")]
        public async Task<ActionResult<IEnumerable<ScheduleController.ViewSRmodel>>> GetList(ScheduleController.SchQry Qry)
        {
            var query = from a in _context.Set<Schedule>()
                        where a.Cle_State == "D1" || a.Cle_State == "C1"
                        join b in _context.Set<Schedule_SB>()
                            on a.Cle_Sch_No equals b.Cle_Sch_No
                        join c in _context.Set<ScrapBooking>()
                            on b.Bookingno equals c.Bookingno
                        join d in _context.Set<UserSpInfo>()
                            on c.Pid equals d.Sbid
                        group new { a } by new { a.Pid, a.Cle_Sch_No, a.Cle_Date, a.Cle_Name, a.Tre_Name, a.Cle_State } into g
                        select new ScheduleController.ViewSRmodel
                        {
                            Pid = g.Key.Pid,
                            Cle_Sch_No = g.Key.Cle_Sch_No,
                            Cle_Date = g.Key.Cle_Date,
                            Cle_Name = g.Key.Cle_Name,
                            Tre_Name = g.Key.Tre_Name,
                            Cle_State = g.Key.Cle_State,
                            Cle_Qty = g.Count()
                        };

            var pred = PredicateBuilder.New<ScheduleController.ViewSRmodel>(true);

            if (Qry.StartDt.HasValue) pred.And(x => x.Cle_Date >= Qry.StartDt.Value);

            if (Qry.EndDt.HasValue) pred.And(x => x.Cle_Date <= Qry.EndDt.Value);

            if (!String.IsNullOrWhiteSpace(Qry.Cle_State)) pred.And(x => x.Cle_State == Qry.Cle_State);
            if (!User.HasRole(RoleList.Admin, RoleList.Epa))
            {
                query = query.Where(x => x.Tre_Name == User.GetCompanyName());
            }
            return await query.Where(pred).ToListAsync();
        }

        [HttpPost]
        [Route("Save")]
        public async Task<IActionResult> Save(Schedule Rec)
        {
            var SC = Rec;
            var _Rec = _context.Schedule.Where(x => x.Pid == SC.Pid).FirstOrDefault();
            _Rec.Pid = SC.Pid;
            _Rec.Enter_Date = SC.Enter_Date;
            _Rec.Full_Weight = SC.Full_Weight;
            _Rec.EmptyCar_Weight = SC.EmptyCar_Weight;
            _Rec.AlFrameY_Qty = SC.AlFrameY_Qty;
            _Rec.AlFrameN_Qty = SC.AlFrameN_Qty;
            _Rec.Cle_State = "C1";
            var _EntityState = EntityState.Modified;
            try
            {
                await _context.SaveChangesAsync();

                var SB = _context.Schedule_SB.Where(x => x.Cle_Sch_No == SC.Cle_Sch_No).ToList();

                foreach (var item in SB)
                {
                    var ListSB = _context.ScrapBooking.Where(x => x.Bookingno == item.Bookingno).FirstOrDefault();

                    ListSB.isSch = "D";
                    _EntityState = EntityState.Modified;
                    await _context.SaveChangesAsync();
                }

            }
            catch (DbUpdateConcurrencyException)
            {
                throw;
            }
            return Ok(new
            {
                Rec = _Rec,
                IsAdded = _EntityState
                
            });
        }

        
        public class ViewSCHmodel
        {
            public Schedule Rec { get; set; }
            public List<string> Bno { get; set; }
            public Dictionary<eItemType, Pvis.Biz.ViewModels.AttachmentViewModel> Attachment { internal get; set; }
        }

    }
}
