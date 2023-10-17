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


namespace Pvis.Web.Controller
{
    [Route("api/[controller]")]
    [ApiController]
    [PvisAuthorize(RoleList.Admin, RoleList.Company)]
    public class UserStoreAddressController : ControllerBase
    {
        private readonly DataDbContext _context;

        public UserStoreAddressController(DataDbContext context)
        {
            _context = context;
        }

        [HttpPost]
        [Route("GetList")]
        public async Task<ActionResult<IEnumerable<UserStoreAddress>>> GetList(USQry Qry)
        {
            var pred = PredicateBuilder.New<UserStoreAddress>(true);
            if (User.HasRole(RoleList.Company))
                pred = pred.And(x => x.Uid == User.GetUid());
            if (!String.IsNullOrWhiteSpace(Qry.Storeaddr))
            {
                Qry.Storeaddr = Qry.Storeaddr.Trim();
                pred = pred.And(x => x.Storeaddr.Contains(Qry.Storeaddr));
            }
            if (!string.IsNullOrEmpty(Qry.Status))
            {
                pred = pred.And(x => x.Status == Qry.Status);
            }
            return await _context.UserStoreAddress.Where(pred).Take(1000).ToListAsync();
        }

        [HttpPost]
        [Route("Load")]
        public async Task<ActionResult<IEnumerable<UserStoreAddress>>> Load()
        {
            if (User.HasRole(RoleList.Company))
                return await _context.UserStoreAddress.Where(x => x.Uid == User.GetUid()).ToListAsync();
            else
                return await _context.UserStoreAddress.ToListAsync();
        }

        [HttpPost]
        [Route("Save")]
        public async Task<IActionResult> Save(UserStoreAddress UserStoreAddress)
        {
            var errors = new List<string>();

            var _EntityState = (UserStoreAddress.Pid <= 0) ? EntityState.Added : EntityState.Modified;
            var Tows = await _context.Town.Where(x =>
            UserStoreAddress.Storeaddr.StartsWith(x.CountyName) &&
            UserStoreAddress.Storeaddr.StartsWith(x.CountyName + x.TownName))
           .Take(2).ToListAsync();

            if (Tows.Count() != 1) errors.Add("輸入地址不正確");
            if (errors.Count > 0) { return BadRequest(new { IsSuccess = false, errors }); }
            if (_EntityState == EntityState.Added)
            {
                if (UserStoreAddressExists(UserStoreAddress.Storeaddr)) errors.Add("此地址已設定過");
                if (errors.Count > 0) { return BadRequest(new { IsSuccess = false, errors }); }
                UserStoreAddress.Createdate = DateTime.Now;
            }
            UserStoreAddress.Uid = User.GetUid();
            //UserStoreAddress.Status = "1";

            _context.Entry(UserStoreAddress).State = _EntityState;
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!UserStoreAddressExists(UserStoreAddress.Pid))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
            return Ok(new
            {
                Rec = UserStoreAddress,
                IsAdded = (_EntityState == EntityState.Added)
            });
        }

        [HttpPost]
        [Route("Delete")]
        public async Task<IActionResult> Delete(UserStoreAddress UserStoreAddress)
        {
            var errors = new List<string>();
            var _UserStoreAddress = await _context.UserStoreAddress.FindAsync(UserStoreAddress.Pid);
            if (_UserStoreAddress == null)
            {
                return NotFound();
            }
            if (UsUsed(UserStoreAddress.Pid)) errors.Add("此地址已被使用，無法刪除!");
            if (errors.Count > 0) { return BadRequest(new { IsSuccess = false, errors }); }

            _context.UserStoreAddress.Remove(_UserStoreAddress);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool UserStoreAddressExists(int id)
        {
            return _context.UserStoreAddress.Any(e => e.Pid == id);
        }

        private bool UserStoreAddressExists(string _storeaddr)
        {
            return _context.UserStoreAddress.Any(e => e.Uid == User.GetUid() && e.Storeaddr == _storeaddr);
        }

        private bool UsUsed(int _pid)
        {
            return _context.ScrapBooking.Any(e => e.Uspid == _pid);
        }

        public class USQry
        {
            public int Pid { get; set; }
            public string Storeaddr { get; set; }
            public string Status { get; set; }

        }
    }
}
