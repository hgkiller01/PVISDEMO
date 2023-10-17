using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Pvis.Biz.CommEnum;
using Pvis.Biz.Member;
using Pvis.Biz.Models;
using Pvis.Biz.Extension;
using Microsoft.EntityFrameworkCore;
using Pvis.Web.Helper;
using Microsoft.AspNetCore.Authorization;

namespace Pvis.Web.Controller
{
    [Route("api/[controller]/[Action]")]
    [ApiController]
    [ValidateAntiForgeryToken]
    [Authorize]
    public class RecordItemController : ControllerBase
    {
        private readonly DataDbContext _context;
        public RecordItemController(DataDbContext context)
        {
            _context = context;
        }
        [HttpPost]
        public async Task<IEnumerable<WasteProd>> GetWasteProds()
        {
            return await _context.WasteProd.ToListAsync();
        }
        [HttpPost]
        public async Task<IEnumerable<RecordItem>> GetRecordItems(BookingID data)
        {
            if (User.HasRole(RoleList.Admin,RoleList.Epa, RoleList.Auditor))
            {
                return await _context.RecordItem.Where(x => x.Aud_Sch_No == data.No).ToListAsync();
            }
            if (User.HasRole(RoleList.Teart,RoleList.Store,RoleList.Company))
            {
                return await _context.RecordItem.Where(x => x.CreateUserID == User.GetUid() && x.Aud_Sch_No == data.No).ToListAsync();
            }
            else
            {
                return new List<RecordItem>();
            }
        }
        [HttpPost]
        public async Task<IActionResult> AddItem(RecordItem recordItem)
        {
            var Audit = _context.ScheduleAudit.Where(x => x.Aud_Sch_No == recordItem.Aud_Sch_No).FirstOrDefault();
            if (Audit != null && Audit.Aud_State == "0")
            {
                Audit.Aud_State = "1";
                _context.SaveChanges();
            }
            recordItem.CreateDate = DateTime.Now;
            recordItem.CreateUserID = User.GetUid();
            recordItem.CreateUserName = User.GetDisplayName();
            recordItem.CompanyName = User.GetCompanyName();
            _context.RecordItem.Add(recordItem);
            await _context.SaveChangesAsync();
            return Ok();
        }
        [HttpPost]
        public async Task<IActionResult> DeleteItem(DataID ID)
        {
            var delete = _context.RecordItem.Where(x => x.RecordIemID == ID.ID).FirstOrDefault();
            var deleteRecord = _context.Record.Where(x => x.RecordItemID == ID.ID);
            _context.RecordItem.Remove(delete);
            _context.Record.RemoveRange(deleteRecord);
            await _context.SaveChangesAsync();
            return Ok();
        }
        [HttpPost]
        public async Task<IActionResult> EditItem(RecordItem recordItem)
        {
            recordItem.ModifyDate = DateTime.Now;
            recordItem.ModifyUserID = User.GetUid();
            recordItem.ModifyUserName = User.GetDisplayName();
            _context.Entry(recordItem).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return Ok();

        }
        public class DataID
        {
            public int ID { get; set; }
        }
        public class BookingID
        {
            public string No { get; set; }
        }
    }
}
