using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Pvis.Biz.CommEnum;
using Pvis.Biz.Models;
using Pvis.Biz.Services;

namespace Pvis.Web.Areas.BackEnd.Pages.Mgr
{
    public class AccountAppReviewModel : PageModel
    {

        private DataDbContext _context;

        public AccountAppReviewModel(DataDbContext context)
        {
            this._context = context;
        }

        public readonly Dictionary<byte, string> AppStatusCode = AccountAppBusinessLayer.AppStatusCode;

        public readonly Dictionary<byte, string> AppRoleCode = AccountAppBusinessLayer.AppRoleCode;

    }
}
