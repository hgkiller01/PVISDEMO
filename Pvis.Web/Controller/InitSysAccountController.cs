using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Pvis.Biz.Member;
using Pvis.Biz.CommEnum;
using System.IO;
using CsvHelper;
using Pvis.Biz.Models;
using Pvis.Biz.Services;
using System.Globalization;

namespace Pvis.Web.Controller
{
    [Route("api/Init20190729/[controller]")]
    [ApiController]
    [LimitSrcIp("127.0.0.1", "::1")]
    public class InitSysAccountController : ControllerBase
    {
        private readonly MemberBusinessLayer _MemberBiz;
        private readonly UserManager<MyAppUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ILogger<InitSysAccountController> _logger;
        private readonly DataDbContext _context;

        public InitSysAccountController(
            RoleManager<IdentityRole> roleManager, 
            UserManager<MyAppUser> userManager, 
            ILogger<InitSysAccountController> logger ,
            ApplicationDbContext applicationDbContext,
            DataDbContext context )
        {
            _MemberBiz = new MemberBusinessLayer(
                userManager,
                roleManager,
                context,
                applicationDbContext
            );
            this._userManager = userManager;
            this._roleManager = roleManager;
            this._logger = logger;
            this._context = context;
        }

        /// <summary>
        /// 取得目前系統的 role 清單
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public IEnumerable<string> Get()
        {
            return _roleManager.Roles.Select(x => x.Name);
        }

        /// <summary>
        /// 建立群組資料,與第一組初始化管理者帳號
        /// </summary>
        /// <returns></returns>
        [HttpPut]
        public async Task<IEnumerable<string>> PutAsync()
        {
            List<string> _Msg = (await _MemberBiz.InitMemberRole()).ToList();
            var _resultUser = await _MemberBiz.SaveAsync(new MyAppUserForm()
            {
                UserName = "sysadmin",
                PhoneNumber = "02-66308000",
                Email = "pvisadmin@eri.com.tw",
                Roles = Enum.GetNames(typeof(RoleList)).ToList().GetRange(0, 1)
            });

            if (_resultUser.errors?.Any() == true )
            {
                _Msg.Add(string.Join(",", _resultUser.errors));
            }
            else
            {
                _Msg.Add($"帳號 {_resultUser.UserName} 已新增密碼 {_resultUser.NewPassWord}");
            }
            _logger.LogInformation(string.Join("\r\n", _Msg));
            return _Msg;
        }

        [HttpGet]
        [Route("InitCode")]
        public async Task<ActionResult> InitCodeAsync() {
            IEnumerable<Town> TownRec = null;
            IEnumerable<County> CountyRec = null;
            List<string> msg = new List<string>();
            if (!_context.County.Any())
            {
                TextReader reader = new StreamReader(Path.Combine(Directory.GetCurrentDirectory(), "CodeTable", "County.csv"));
                var csvReader = new CsvReader(reader, CultureInfo.InvariantCulture);
                CountyRec = csvReader.GetRecords<County>();
                _context.AddRange(CountyRec);
                msg.Add("縣市代碼已新增");
            }
            else {
                msg.Add("縣市代碼已存在");
            }

            if (!_context.Town.Any())
            {
                TextReader reader = new StreamReader(Path.Combine(Directory.GetCurrentDirectory(), "CodeTable", "Town.csv"));
                //var csvReader = new CsvReader(reader);
                var csvReader = new CsvReader(reader, CultureInfo.InvariantCulture);
                TownRec = csvReader.GetRecords<Town>();
                _context.AddRange(TownRec);
                msg.Add("鄉鎮市區代碼已新增");
            }
            else {
                msg.Add("鄉鎮市區代碼已存在");
            }

            if (TownRec != null || CountyRec != null)
            {
                await _context.SaveChangesAsync();
            }

            return Ok(new
            {
                msg
            });

        }

    }
}
