using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchoopFunctionApp.Models
{
    public class CollatedAlerts
    {
        public int schoopID { get; set; }

        public int alertID { get; set; }

        public string alertDateTime { get; set; }

        public string schoolName { get; set; }

        public string activeYears { get; set; }

        public string activeGroups { get; set; }

        public string alertText { get; set; }

        public bool alertUrgent { get; set; }

        public int alertIcon { get; set; }

        public int nID { get; set; } //news id
        public int eID { get; set; } //event id
        public string fID { get; set; } //survey url
        public int sc { get; set; } //suggest schoopID

        public CollatedAlerts(int schoopId, string school_name, int alert_ID, string alert_date_time, string active_years, string active_groups, bool alert_urgent, string alert_text, int alert_Icon, int nid, int eid, string fid, int suugestSchoopId)
        {
            schoopID = schoopId;
            schoolName = school_name;
            alertID = alert_ID;
            alertDateTime = alert_date_time;
            activeYears = string.IsNullOrEmpty(active_years) ? "" : active_years;
            alertUrgent = alert_urgent;
            alertText = string.IsNullOrEmpty(alert_text) ? "" : alert_text;
            alertIcon = alert_Icon;
            activeGroups = active_groups;
            nID = nid;
            eID = eid;
            fID = fid;
            sc = suugestSchoopId;
        }
    }
}
