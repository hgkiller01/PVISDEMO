using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using AutoMapper;
using LinqKit;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Pvis.Biz.CommEnum;
using Pvis.Biz.Extension;
using Pvis.Biz.Member;
using Pvis.Biz.Models;

namespace Pvis.Biz.Services
{
    public class MemberBusinessLayer
    {

        static MemberBusinessLayer()
        {
            var configuration = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<MyAppUserForm, MyAppUser>();
                cfg.CreateMap<MyAppUser, MyAppUserForm>();
                cfg.CreateMap<MyAppUserFormProfile, MyAppUser>();
                cfg.CreateMap<MyAppUser, MyAppUserFormProfile>();
            });
            mapper = configuration.CreateMapper();
        }

        private readonly UserManager<MyAppUser> userManager;
        private readonly RoleManager<IdentityRole> roleManager;
        private readonly DataDbContext context;
        private readonly ApplicationDbContext applicationDbContext;
        public static readonly IMapper mapper;

        public MemberBusinessLayer(
            UserManager<MyAppUser> userManager,
            RoleManager<IdentityRole> roleManager,
            DataDbContext context,
            ApplicationDbContext applicationDbContext)
        {
            this.userManager = userManager;
            this.roleManager = roleManager;
            this.context = context;
            this.applicationDbContext = applicationDbContext;
        }

        public async Task<IEnumerable<MyAppUser>> GetListAsync(MemberQry Qry)
        {
            var pred = PredicateBuilder.New<MyAppUser>(true);

            if (!string.IsNullOrWhiteSpace(Qry.Role))
            {
                var role = (RoleList)Enum.Parse(typeof(RoleList), Qry.Role, true);
                pred.And(x => x.Role.HasFlag(role));
            }
            if (!string.IsNullOrWhiteSpace(Qry.KeyWord))
            {
                pred.And(x => x.UserName.Contains(Qry.KeyWord) || x.DisplayName.Contains(Qry.KeyWord) || x.CompanyName.Contains(Qry.KeyWord));
            }
            return await applicationDbContext.Users.Where(pred)
                .OrderByDescending(x => x.Uid)
                .Take(Qry.TopN).ToArrayAsync();

        }

        public async Task<MyAppUserFormProfile> GetUserProfileAsync(ClaimsPrincipal user)
        {
            return mapper.Map<MyAppUserFormProfile>(await userManager.GetUserAsync(user));
        }

        public async Task<IEnumerable<string>> InitMemberRole()
        {
            List<string> _Roles = Enum.GetNames(typeof(RoleList)).ToList();
            List<string> _msg = new List<string>();
            
            foreach (var role in _Roles)
            {
                if (await roleManager.RoleExistsAsync(role))
                {
                    _msg.Add($"Role {role}已存在");
                }
                else
                {
                    await roleManager.CreateAsync(new IdentityRole() { Name = role });
                    _msg.Add($"Role {role}已新增");
                }
            }
            return _msg;
        }

        /// <summary>
        /// 單筆資料取得
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<MyAppUserForm> GetItemAsync(MyAppUserForm rec)
        {
            var User = await userManager.FindByIdAsync(rec.Id);

            if (User == null) return null;

            var Result = mapper.Map<MyAppUserForm>(User);

            Result.Roles = await userManager.GetRolesAsync(User);

            return Result;
        }


        /// <summary>
        /// 重設密碼
        /// </summary>
        /// <param name="rec"></param>
        /// <returns></returns>
        public async Task<ActionResult<MyAppUserForm>> ResetPassWordAsync(MyAppUserForm rec)
        {
            var _Rec = await userManager.FindByIdAsync(rec.Id);

            if (_Rec == null) return null;

            var _Token = await userManager.GeneratePasswordResetTokenAsync(_Rec);

            string NewPassWord = Guid.NewGuid().ToString().Replace("-", "").Substring(10) + "A@";

            var _ResetResult = await userManager.ResetPasswordAsync(_Rec, _Token, NewPassWord);

            var Result = mapper.Map<MyAppUserForm>(_Rec);

            Result.NewPassWord = NewPassWord;

            return Result;
        }

        /// <summary>
        /// 新增異動處理
        /// </summary>
        /// <param name="Rec"></param>
        /// <returns></returns>
        public async Task<MyAppUserForm> SaveAsync(MyAppUserForm Rec)
        {
            Rec.errors = new List<string>();
            //if (applicationDbContext.Users.Any(x =>
            //   x.Email.Equals(Rec.Email, StringComparison.OrdinalIgnoreCase) &&
            //   x.Id != Rec.Id
            //)) Rec.errors.Add("此組Email已經有別的帳號使用");

            //if (context.AccountApp.Any(x =>
            //   x.Email.Equals(Rec.Email, StringComparison.OrdinalIgnoreCase) &&
            //   x.Status != AppStatusList.accept &&
            //   !Rec.UserName.Equals(x.ControlNo,StringComparison.OrdinalIgnoreCase)
            //)) Rec.errors.Add("此組Email已經有別的申請資料使用");

            if (Rec.errors.Count > 0) return Rec;

            MyAppUser User = await userManager.FindByIdAsync(Rec.Id);
            if (User != null)
            {
                User = mapper.Map<MyAppUserForm, MyAppUser>(Rec, User);
                User.Role = 0;
                Rec.Roles.ToList().ForEach(x => User.Role |= (RoleList)Enum.Parse(typeof(RoleList), x, true));
                var _updateResult = await userManager.UpdateAsync(User);
                await SetUserRolesAsync(User, Rec.Roles);
                return mapper.Map<MyAppUserForm>(User);
            }

            if (await userManager.FindByNameAsync(Rec.UserName) != null)
            {
                Rec.errors.Add("此組帳號已經使用");
                return Rec;
            }

            string randpwd = Guid.NewGuid().ToString().Replace("-", "").Substring(10) + "A@";
            User = mapper.Map<MyAppUser>(Rec);
            User.Role = 0;
            User.Id = Guid.NewGuid().ToString().ToLower();
            Rec.Roles.ToList().ForEach(x => User.Role |= (RoleList)Enum.Parse(typeof(RoleList), x, true));
            var _createResult = await userManager.CreateAsync(User, randpwd);
            await SetUserRolesAsync(User, Rec.Roles);
            Rec = mapper.Map<MyAppUserForm>(User);
            Rec.NewPassWord = randpwd;

            return Rec;
        }

        /// <summary>
        /// 刪除使用者
        /// </summary>
        /// <param name="Rec"></param>
        /// <returns></returns>
        public async Task<bool> DeleteAsync(MyAppUserForm Rec) {
            MyAppUser User = await userManager.FindByIdAsync(Rec.Id);
            if (User == null) return false;
            var Result = await userManager.DeleteAsync(User);
            return Result.Succeeded;
        }

        private async Task SetUserRolesAsync(MyAppUser user, IList<string> newRoles)
        {
            var oldRoles = await userManager.GetRolesAsync(user);
            await userManager.RemoveFromRolesAsync(user, oldRoles.Where(x => newRoles.IndexOf(x) < 0).ToList());
            await userManager.AddToRolesAsync(user, newRoles.Where(x => oldRoles.IndexOf(x) < 0).ToList());
        }

        public static Dictionary<string, string> MemberRoleCode
        {
            get
            {
                var code = new Dictionary<string, string>();
                foreach (RoleList role in Enum.GetValues(typeof(RoleList)))
                {
                    code.Add(role.ToString(), EnumHelper<RoleList>.GetDisplayValue(role));
                }
                return code;
            }
        }

    }

    public class MemberQry
    {
        public String KeyWord { get; set; }
        public String Role { get; set; }
        [Range(0, 2000, ErrorMessage = "超出單次查詢筆數上線")]
        public int TopN { get; set; } = 1000;
    }

    public class MyAppUserForm
    {
        public string Id { get; set; }
        [Required(ErrorMessage = "登入帳號為必填")]
        [RegularExpression("^[A-Za-z0-9@_\\.]{4,50}$", ErrorMessage = "帳號最少4碼,最多50碼")]
        public string UserName { get; set; }
        [Required(ErrorMessage = "姓名為必填")]
        [MaxLength(250, ErrorMessage = "姓名最多250個字")]
        [MinLength(2, ErrorMessage = "姓名至少2個字")]
        public string DisplayName { get; set; }
        [Required(ErrorMessage = "機構名稱為必填")]
        [MaxLength(250, ErrorMessage = "機構名稱最多250個字")]
        [MinLength(2, ErrorMessage = "機構名稱至少2個字")]
        public string CompanyName { get; set; }
        [EmailAddress(ErrorMessage = "Email格式錯誤")]
        [Required(ErrorMessage = "Email為必填")]
        public string Email { get; set; }
        public int Uid { get; set; }
        //[RegularExpression("([0-9]{9,10}|\\(?[0-9]{0,1}[0-9]{1,2}\\)?-?[0-9]{1,20})", ErrorMessage = "電話格式錯誤")]
        [Required(ErrorMessage = "聯絡電話為必填")]
        public string PhoneNumber { get; set; }
        public string NewPassWord { get; set; }
        public IList<string> Roles { get; set; }

        public IList<string> errors { get; set; }
        public int? AppPid { get; set; }

        public string CaseName { get; set; }
        public string CaseEmail { get; set; }
        public string IPAddress { get; set; }
        [StringLength(250)]
        public string Address { get; set; }
    }


    public class MyAppUserFormProfile
    {
        public string Id { get; set; }
        [MaxLength(250, ErrorMessage = "姓名最多250個字")]
        [MinLength(2, ErrorMessage = "姓名至少2個字")]
        public string DisplayName { get; set; }
        [MaxLength(250, ErrorMessage = "機構名稱最多250個字")]
        [MinLength(2, ErrorMessage = "機構名稱至少2個字")]
        public string CompanyName { get; set; }
        [EmailAddress(ErrorMessage = "Email格式錯誤")]
        [Required(ErrorMessage = "Email為必填")]
        public string Email { get; set; }
        [Phone(ErrorMessage = "電話格式錯誤")]
        [Required(ErrorMessage = "聯絡電話為必填")]
        public string PhoneNumber { get; set; }
        public IList<string> errors { get; set; }
        public string CaseName { get; set; }
        public string CaseEmail { get; set; }
    }
}
