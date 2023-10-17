using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Pvis.Biz.Models;
using Microsoft.AspNetCore.Mvc;
using Pvis.Biz.Member;

namespace Pvis.Web.Areas.BackEnd.Pages.Apply
{
    public class ScrapBookingVue : PageModel
    {

        public void OnPost(string Bno)
        {
            ViewData["BNO"] = Bno;
        }

    }
}
