using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchoopFunctionApp.Entities
{
    public partial class tbl_school_alerts
    {
        public int Alert_id { get; set; }
        public int Schoop_id { get; set; }
        public Nullable<System.DateTime> Alert_date_time { get; set; }
        public string Activeyears { get; set; }
        public bool Alert_urgent { get; set; }
        public string Alert_text { get; set; }
        public int Alert_sent_by { get; set; }
        public Nullable<int> Language_id { get; set; }
        public string ActiveGroups { get; set; }
        public bool IsSent { get; set; }
        public bool IsLoginAs { get; set; }
        public bool IsTranslatedByGoogle { get; set; }
        public Nullable<System.Guid> SurveyGuid { get; set; }
        public bool IsScheduled { get; set; }
        public Nullable<System.DateTime> ToSendTime { get; set; }
        public Nullable<int> ParentAlertId { get; set; }
        public Nullable<int> EventId { get; set; }
        public Nullable<int> NewsId { get; set; }
        public bool Deleted { get; set; }
        public bool SendByDeviceIds { get; set; }
        public string DeviceIds { get; set; }
        public bool suggestedChannel { get; set; }
        public Nullable<int> suggestedSchoopID { get; set; }
        public bool IsPrivate { get; set; }
        public bool IsContactFormAlert { get; set; }
        public bool IsMigrateAlert { get; set; }
        public bool IsInDocumentsDB { get; set; }
        public bool IsSentForDeviceType5 { get; set; }
    }
}
