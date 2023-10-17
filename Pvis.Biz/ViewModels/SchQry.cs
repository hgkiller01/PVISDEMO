using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Pvis.Biz.Models;

namespace Pvis.Biz.ViewModels
{
    public class SchQry
    {
        public int Pid { get; set; }
        public string City { get; set; }
        public int Uspid { get; set; }
        public DateTime? StartDt { get; set; }
        public DateTime? EndDt { get; set; }
        public string Aud_Sch_No { get; set; }
        public string Bookingno { get; set; }
        public string Aud_State { get; set; }
    }
    public class ViewSAmodel
    {
        public ScheduleAudit Rec { get; set; }
        public List<string> Bno { get; set; }
    }
}
