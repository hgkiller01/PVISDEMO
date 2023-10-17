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

namespace Pvis.Web.Controller
{
    [Route("api/[controller]")]
    [ApiController]
    [PvisAuthorize(RoleList.Admin, RoleList.Auditor)]
    public class ClearLocationController : ControllerBase
    {
        private readonly DataDbContext _context;

        public ClearLocationController(DataDbContext context)
        {
            _context = context;
        }

        [HttpPost]
        [Route("GetList")]
        public async Task<ActionResult<IEnumerable<ClearLocation>>> GetList(Cond Qry)
        {
            var pb = PredicateBuilder.New<ClearLocation>(true);

            if (!string.IsNullOrWhiteSpace(Qry.KeyWord))
            {
                Qry.KeyWord = Qry.KeyWord.Trim();
                pb = pb.And(x => x.Name.Contains(Qry.KeyWord) || x.Addr.Contains(Qry.KeyWord));
            }
            if (Qry.Status.HasValue)
            {
                pb = pb.And(x => x.Status == Qry.Status);
            }

            return await _context.ClearLocation.Where(pb).Take(1000).ToListAsync();
        }

        [HttpPost]
        [Route("GetItem")]
        public async Task<ActionResult<ClearLocation>> GetItem(Cond Qry)
        {
            var item = await _context.ClearLocation.FirstOrDefaultAsync(x => x.Pid == Qry.Pid);

            return Ok(new
            {
                IsSuccess = (item != null),
                Rec = item
            });
        }

        [HttpPost]
        [Route("Save")]
        public async Task<IActionResult> Save(ClearLocation item)
        {
            var State = (item.Pid > 0) ? EntityState.Modified : EntityState.Added;

            if (State == EntityState.Added)
            {
                item.InsDT = DateTime.Now;
                item.InsUid = User.GetUid();
            }
            item.UpdDT = DateTime.Now;
            item.UpdUid = User.GetUid();

            _context.Entry(item).State = State;
            await _context.SaveChangesAsync();

            return Ok(new
            {
                Rec = item,
                IsAdded = (State == EntityState.Added)
            });
        }

        [HttpPost]
        [Route("Delete")]
        public async Task<IActionResult> Delete(ClearLocation item)
        {
            _context.ClearLocation.Remove(item);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool ClearLocationExists(int id)
        {
            return _context.ClearLocation.Any(x => x.Pid == id);
        }

        public class Cond
        {
            public int Pid { get; set; }
            public string KeyWord { get; set; }
            public bool? Status { get; set; }
        }
    }
}
