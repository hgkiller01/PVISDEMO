using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using LinqKit;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Pvis.Biz.CommEnum;
using Pvis.Biz.Extension;
using Pvis.Biz.Member;
using Pvis.Biz.Models;
using Pvis.Biz.Services;
using Pvis.Web.Helper;

namespace Pvis.Web.Controller
{
    [Route("api/[controller]")]
    [ApiController]
    [PvisAuthorize(RoleList.Admin, RoleList.Epa, RoleList.Company)]
    public class ScheduleDocController : ControllerBase
    {
        private readonly DataDbContext _context;

        public ScheduleDocController(DataDbContext context)
        {
            _context = context;
        }

        [HttpPost]
        [Route("Load")]
        public async Task<ActionResult<IEnumerable<ScheduleController.ViewSRmodel>>> Load()
        {
            var query = from a in _context.Set<Schedule>()
                        join b in _context.Set<Schedule_SB>()
                            on a.Cle_Sch_No equals b.Cle_Sch_No
                        join c in _context.Set<ScrapBooking>()
                            on b.Bookingno equals c.Bookingno
                        join d in _context.Set<UserSpInfo>()
                            on c.Pid equals d.Sbid
                        group new { a,c } by new { a.Pid, a.Cle_Sch_No, a.Cle_Date, a.Cle_Name, a.Tre_Name, a.Cle_State ,c.Uid } into g
                        select new ScheduleController.ViewSRmodel
                        {
                            Pid = g.Key.Pid,
                            Cle_Sch_No = g.Key.Cle_Sch_No,
                            Cle_Date = g.Key.Cle_Date,
                            Cle_Name = g.Key.Cle_Name,
                            Tre_Name = g.Key.Tre_Name,
                            Cle_State = g.Key.Cle_State,
                            Uid = g.Key.Uid,
                            Cle_Qty = g.Count()
                        };

            var pred = PredicateBuilder.New<ScheduleController.ViewSRmodel>(true);
            ScheduleController.SchQry Qry = new ScheduleController.SchQry();
            //Qry.Cle_State = "1";
            if (!String.IsNullOrWhiteSpace(Qry.Cle_State)) pred.And(x => x.Cle_State == Qry.Cle_State);
            if (!User.HasRole(RoleList.Admin, RoleList.Epa, RoleList.Auditor))
            {
                query = query.Where(x => x.Uid == User.GetUid());
            }

            return await query.Where(pred).ToListAsync();
        }

        [HttpPost]
        [Route("GetList")]
        public async Task<ActionResult<IEnumerable<ScheduleController.ViewSRmodel>>> GetList(ScheduleController.SchQry Qry)
        {
            var query = from a in _context.Set<Schedule>()
                        join b in _context.Set<Schedule_SB>()
                            on a.Cle_Sch_No equals b.Cle_Sch_No
                        join c in _context.Set<ScrapBooking>()
                            on b.Bookingno equals c.Bookingno
                        join d in _context.Set<UserSpInfo>()
                            on c.Pid equals d.Sbid
                        group new { a } by new { a.Pid, a.Cle_Sch_No, a.Cle_Date, a.Cle_Name, a.Tre_Name, a.Cle_State, c.Uid } into g
                        select new ScheduleController.ViewSRmodel
                        {
                            Pid = g.Key.Pid,
                            Cle_Sch_No = g.Key.Cle_Sch_No,
                            Cle_Date = g.Key.Cle_Date,
                            Cle_Name = g.Key.Cle_Name,
                            Tre_Name = g.Key.Tre_Name,
                            Cle_State = g.Key.Cle_State,
                            Uid = g.Key.Uid,
                            Cle_Qty = g.Count()
                        };

            var pred = PredicateBuilder.New<ScheduleController.ViewSRmodel>(true);

            if (Qry.StartDt.HasValue) pred.And(x => x.Cle_Date >= Qry.StartDt.Value);

            if (Qry.EndDt.HasValue) pred.And(x => x.Cle_Date <= Qry.EndDt.Value);

            if (!String.IsNullOrWhiteSpace(Qry.Cle_State)) pred.And(x => x.Cle_State == Qry.Cle_State);

            if (!User.HasRole(RoleList.Admin, RoleList.Epa, RoleList.Auditor))
            {
                query = query.Where(x => x.Uid == User.GetUid());
            }
            return await query.Where(pred).ToListAsync();
        }

        [HttpPost]
        [Route("GetItem")]
        public async Task<ActionResult<IEnumerable<Schedule>>> GetItem(ScheduleController.SchQry Qry)
        {
            var _Rec = await _context.Schedule.Where(x => x.Pid == Qry.Pid).FirstOrDefaultAsync();
            Qry.Cle_Sch_No = _Rec.Cle_Sch_No;
            List<Schedule_SB> ListSS = null;
            List<ScrapBooking> ListSB = new List<ScrapBooking>();
            List<ScrapBooking> ListA = null;
            int Sum_Qty = 0;
            decimal Sum_Weight = 0;

            if (_Rec != null && _Rec.Pid > 0)
            {

                ListSS = await _context.Schedule_SB.Where(x => x.Cle_Sch_No == Qry.Cle_Sch_No).ToListAsync();
                foreach (var item in ListSS)
                {
                    ListSB = _context.ScrapBooking.Where(x => x.Bookingno == item.Bookingno).ToList();
                    if (ListA == null)
                        ListA = ListSB;
                    else
                        ListSB.AddRange(ListA);
                }
                var usa = await _context.UserStoreAddress.ToListAsync();
                foreach (var item in ListSB)
                {
                    item.USAddr1 = usa.FirstOrDefault(x => x.Pid == item.Uspid).Storeaddr;
                    item.Qty = await _context.UserSpInfo.Where(x => x.Sbid == item.Pid).CountAsync();
                    var sp = await _context.UserSpInfo.Where(x => x.Sbid == item.Pid).ToListAsync();
                    item.SPWeight1 = sp.Select(x => x.Spweight).Sum();
                    item.Al_frameYQty = await _context.UserSpInfo.Where(x => x.Sbid == item.Pid && x.AlFrame == "1").CountAsync();
                    item.Al_frameNQty = await _context.UserSpInfo.Where(x => x.Sbid == item.Pid && x.AlFrame == "0").CountAsync();
                    Sum_Qty += Convert.ToInt32(item.Qty);
                    Sum_Weight += Convert.ToDecimal(item.SPWeight1);
                }
                _Rec.Sum_Qty = Sum_Qty;
                _Rec.Sum_Weight = Sum_Weight;

            }

            return Ok(new
            {
                IsSuccess = (_Rec != null),
                Rec = _Rec,
                RecSchSBList = ListSB
            });
        }

        [HttpPost]
        [Route("GetView")]
        public async Task<ActionResult<Schedule>> View(ScheduleController.SchQry Qry)
        {
            var _Rec = await _context.Schedule.Where(x => x.Pid == Qry.Pid).FirstOrDefaultAsync();
            Qry.Cle_Sch_No = _Rec.Cle_Sch_No;
            List<Schedule_SB> ListSS = null;
            List<ScrapBooking> ListSB = new List<ScrapBooking>();
            List<ScrapBooking> ListA = null;
            List<Schedule_Review> ListSCR = null;
            int Sum_Qty = 0;
            decimal Sum_Weight = 0;
            if (_Rec != null && _Rec.Pid > 0)
            {

                ListSS = await _context.Schedule_SB.Where(x => x.Cle_Sch_No == Qry.Cle_Sch_No).ToListAsync();
                foreach (var item in ListSS)
                {
                    ListSB = _context.ScrapBooking.Where(x => x.Bookingno == item.Bookingno).ToList();
                    if (ListA == null)
                        ListA = ListSB;
                    else
                        ListSB.AddRange(ListA);
                }
                var usa = await _context.UserStoreAddress.ToListAsync();
                foreach (var item in ListSB)
                {
                    item.USAddr1 = usa.FirstOrDefault(x => x.Pid == item.Uspid).Storeaddr;
                    item.Qty = await _context.UserSpInfo.Where(x => x.Sbid == item.Pid).CountAsync();
                    var sp = await _context.UserSpInfo.Where(x => x.Sbid == item.Pid).ToListAsync();
                    item.SPWeight1 = sp.Select(x => x.Spweight).Sum();
                    item.Al_frameYQty = await _context.UserSpInfo.Where(x => x.Sbid == item.Pid && x.AlFrame == "1").CountAsync();
                    item.Al_frameNQty = await _context.UserSpInfo.Where(x => x.Sbid == item.Pid && x.AlFrame == "0").CountAsync();
                    Sum_Qty += Convert.ToInt32(item.Qty);
                    Sum_Weight += Convert.ToDecimal(item.SPWeight1);
                }
                _Rec.Sum_Qty = Sum_Qty;
                _Rec.Sum_Weight = Sum_Weight;
                _Rec.FileCleDoc = await _context.FormFileUpload.Where(x => x.AppId == Qry.Pid.ToString()
                                    && x.DocType == eDocType.ScheduleDoc
                                    && x.ItemType == eItemType.CleDoc
                                  ).ToListAsync();
                _Rec.FileTreDoc = await _context.FormFileUpload.Where(x => x.AppId == Qry.Pid.ToString()
                                    && x.DocType == eDocType.ScheduleDoc
                                    && x.ItemType == eItemType.TreDoc
                                  ).ToListAsync();
                ListSCR = await _context.Schedule_Review.Where(x => x.Cle_Sch_No == Qry.Cle_Sch_No).OrderByDescending(x => x.Check_Date).ThenByDescending(x => x.Pid).ToListAsync();

            }
            return Ok(new
            {
                IsSuccess = (_Rec != null),
                Rec = _Rec,
                RecSchSBList = ListSB,
                ReviewList = ListSCR
            });
        }

        [HttpPost]
        [Route("Save")]
        public async Task<IActionResult> Save(ViewSCHmodel SCH)
        {
            var SC = SCH.Rec;

            var _EntityState = SC.Pid > 0 ? EntityState.Modified : EntityState.Added;

            try
            {
                
                foreach (var item in SCH.Attachment)
                {
                    if (_EntityState != EntityState.Added) { await DeleteAttached(new ScheduleController.SchQry { Pid = SCH.Rec.Pid }); }
                    var F = FormFileUploadBusinessLayer.FillFileData(item.Value, new FormFileUpload() { AppId = SC.Pid.ToString(), DocType = eDocType.ScheduleDoc, ItemType = item.Key }, User.GetUid());
                    if (FileCheck.IsAllowedExtension(item.Value,F,_context,"pdf", "上傳清理合約", 
                        Request.HttpContext.Connection.RemoteIpAddress.ToString()))
                    {
                        try
                        {
                            FormFileUploadBusinessLayer.SetFilePath(F);
                            Directory.CreateDirectory(Path.GetDirectoryName(FormFileUploadBusinessLayer.GetSavePath(F)));
                            System.IO.File.WriteAllBytes(FormFileUploadBusinessLayer.GetSavePath(F), item.Value.GetContent());
                            _context.Add(F).State = EntityState.Added;
                        }
                        catch (Exception ex)
                        {
                            FileCheck.WriteErrorFile(F,_context, ex.Message, "上傳清理合約", Request.HttpContext.Connection.RemoteIpAddress.ToString());
                        }
                        await _context.SaveChangesAsync();
                    }

                }

                SC.Cle_State = "D";
                _context.Add(SC).State = EntityState.Modified;
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                throw;
            }
            return Ok(new
            {
                Rec = SC,
                IsAdded = (_EntityState == EntityState.Added)
            });
        }

        [HttpPost]
        [Route("DeleteAttached")]
        public async Task<IActionResult> DeleteAttached(ScheduleController.SchQry item)
        {
            var F = _context.FormFileUpload.Where(x =>
                    x.AppId == item.Pid.ToString() &&
                    x.DocType == eDocType.ScheduleDoc &&
                    x.ItemType == eItemType.None
                ).FirstOrDefault();
            if (F == null)
            {
                return Ok();
            }
            var _SavePath = FormFileUploadBusinessLayer.GetSavePath(F);
            if (System.IO.File.Exists(_SavePath))
            {
                System.IO.File.Delete(_SavePath);
            }
            _context.Entry(F).State = EntityState.Deleted;
            await _context.SaveChangesAsync();
            return Ok();
        }


        public class ViewSCHmodel
        {
            public Schedule Rec { get; set; }
            public List<string> Bno { get; set; }
            public Dictionary<eItemType, Pvis.Biz.ViewModels.AttachmentViewModel> Attachment { internal get; set; }
        }
    }
}
