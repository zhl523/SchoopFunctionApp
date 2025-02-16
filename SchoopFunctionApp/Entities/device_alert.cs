using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchoopFunctionApp.Entities
{
    public class device_alert
    {
        public int Alert_id { get; set; }
        public DateTime? Alert_date_time { get; set; }
        public int? Language_id { get; set; }
        public int Schoop_id { get; set; }
        public string Activeyears { get; set; }
        public string Alert_text { get; set; }
        public bool Alert_urgent { get; set; }
        public int? EventId { get; set; }
        public int? NewsId { get; set; }
        public Guid? SurveyGuid { get; set; }
        public string ActiveGroups { get; set; }
        //public string ActiveGroupNames { get; set; } //TODO later, generate names from DB function
        public int? suggestedSchoopID { get; set; }
        public string CustomDateFormat { get; set; }
        public string EstablishmentName { get; set; }
        public bool SendByDeviceIds { get; set; }
        public bool IsContactFormAlert { get; set; }
        public bool IsMigrateAlert { get; set; }
    }
}
