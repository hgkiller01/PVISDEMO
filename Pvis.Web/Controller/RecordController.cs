using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Pvis.Biz.Member;
using Pvis.Biz.CommEnum;
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
    public class RecordController : ControllerBase
    {
        private readonly DataDbContext _context;
        public RecordController(DataDbContext context)
        {
            _context = context;
        }
        public async Task<IEnumerable<RecordViewModel>> GetRecords(ItemNo itemNo)
        {
            var data = (from record in _context.Record
                        join ritem in _context.RecordItem
                        on record.RecordItemID equals ritem.RecordIemID
                        where record.Aud_Sch_No == itemNo.No
                        select new { record, ritem });
            if (!User.HasRole(RoleList.Admin,RoleList.Epa,RoleList.Auditor))
            {
                data = data.Where(x => x.record.CreateUserID == User.GetUid());
            }
            var model = await data.Select(x => new RecordViewModel()
                            {
                                RecordID = x.record.RecordID,
                                RecordItemID = x.record.RecordItemID,
                                Code_name = x.ritem.Code_name,
                                WasteSn = x.record.WasteSn,
                                VendorName = x.record.VendorName,
                                UseFor = x.record.UseFor,
                                ExportNumber = x.record.ExportNumber,
                                ShipmentDate = x.record.ShipmentDate,
                                CountryName = x.record.CountryName,
                                ContainerSn = x.record.ContainerSn,
                                CreateDate = x.record.CreateDate,
                                Code_Type = x.ritem.Code_Type,
                                Code_no = x.ritem.Code_no,
                                RecordWeight = x.record.RecordWeight,
                                CompanyName = x.record.CompanyName,
                                CreateUserID = x.record.CreateUserID,
                                CreateUserName = x.record.CreateUserName,
                                ItemName = x.ritem.ItemName
                            }).ToListAsync();
            return model;
        }
        public async Task<IEnumerable<SelectItem>> GetSelectItems(ItemNo itemNo)
        {
            var items = await _context.RecordItem.Where(x => x.Aud_Sch_No == itemNo.No)
            .Select(x => new SelectItem()
            {
                RecordItemID = x.RecordIemID,
                Code_name = x.Code_name,
                Code_no = x.Code_no,
                Code_Type = x.Code_Type,
                ItemName = x.ItemName,
                Uid = x.CreateUserID
            }).ToListAsync();
            if (!User.HasRole(RoleList.Admin, RoleList.Epa, RoleList.Auditor))
            {
                items = items.Where(x => x.Uid == User.GetUid()).ToList();
            }

            return  items;
        }
        public async Task<IActionResult> AddItem(Record record)
        {
            var Audit = _context.ScheduleAudit.Where(x => x.Aud_Sch_No == record.Aud_Sch_No).FirstOrDefault();
            if (Audit != null && Audit.Aud_State == "0")
            {
                Audit.Aud_State = "1";
                _context.SaveChanges();
            }
            record.CreateDate = DateTime.Now;
            record.CreateUserID = User.GetUid();
            record.CreateUserName = User.GetDisplayName();
            record.CompanyName = User.GetCompanyName();
            _context.Record.Add(record);
            try
            {
                await _context.SaveChangesAsync();
            }
            catch(Exception e)
            {
                string a = e.Message;
            }
            
            return Ok();
        }
        public async Task<IActionResult> EditItem(Record record)
        {
            record.ModifyDate = DateTime.Now;
            record.ModifyUserID = User.GetUid();
            record.ModifyUserName = User.GetDisplayName();
            _context.Entry(record).State = EntityState.Modified;
            await _context.SaveChangesAsync();                           
            return Ok();
        }
        public async Task<IActionResult> DeleteItem(DelteID record)
        {
            var Ditem = _context.Record.Find(record.RecordID);
            _context.Record.Remove(Ditem);
            await _context.SaveChangesAsync();
            return Ok();
        }
        public async Task<ScheduleAudit> GetSumbitDate(ItemNo itemNo)
        {
            return await _context.ScheduleAudit.Where(x => x.Aud_Sch_No == itemNo.No).FirstOrDefaultAsync();
        }
        public class DelteID
        {
            public int RecordID { get; set; }
        }
        public class ItemNo
        {
            public string No { get; set; }
        }
        public class SelectItem 
        {
            public int RecordItemID { get; set; }
            public string Code_name { get; set; }
            public string Code_Type { get; set; }
            public string Code_no { get; set; }
            public string ItemName { get; set; }
            public int Uid { get; set; }
        }
        public class RecordViewModel
        {
            public int RecordID { get; set; }
            public int RecordItemID { get; set; }
            public string Code_Type { get; set; }
            public string Code_no { get; set; }
            public string Code_name { get; set; }
            public double? RecordWeight { get; set; }
            public string WasteSn { get; set; }
            public string VendorName { get; set; }
            public string UseFor { get; set; }
            public string ExportNumber { get; set; }
            public DateTime? ShipmentDate { get; set; }
            public string CountryName { get; set; }
            public string ContainerSn { get; set; }
            public string ItemName { get; set; }
            public DateTime CreateDate { get; set; }
            public int CreateUserID { get; set; }
            public string CreateUserName { get; set; }
            public string CompanyName { get; set; }
            
        }
    }
}
