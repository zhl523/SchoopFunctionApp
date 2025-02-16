using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchoopFunctionApp.Entities
{
    public partial class tbl_school_events
    {
        public int Event_id { get; set; }
        public int SchoopID { get; set; }
        public string Event_title { get; set; }
        public System.DateTime Event_start_date { get; set; }
        public System.DateTime Event_end_date { get; set; }
        public System.DateTime Event_start_time { get; set; }
        public System.DateTime Event_end_time { get; set; }
        public string Event_location { get; set; }
        public Nullable<decimal> Event_cost { get; set; }
        public string Event_text { get; set; }
        public string Event_contact_details { get; set; }
        public bool Event_active { get; set; }
        public int Language_id { get; set; }
        public string ActiveYears { get; set; }
        public Nullable<int> CategoryId { get; set; }
        public string ICSUID { get; set; }
        public Nullable<int> FeedTypeId { get; set; }
        public Nullable<System.DateTime> UpdatedOn { get; set; }
        public string ActiveGroups { get; set; }
        public bool IsTranslatedByGoogle { get; set; }
        public bool Deleted { get; set; }
        public bool IsPrivate { get; set; }
        public bool SendByDeviceIds { get; set; }
        public string DeviceIds { get; set; }
    }
}
