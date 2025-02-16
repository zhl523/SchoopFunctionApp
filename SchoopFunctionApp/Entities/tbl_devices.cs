using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchoopFunctionApp.Entities
{
    public partial class tbl_devices
    {
        public int deviceID { get; set; }
        public string deviceToken { get; set; }
        public byte deviceTypeID { get; set; }
        public string deviceOSVersion { get; set; }
        public string deviceName { get; set; }
        public string deviceEmail { get; set; }
        public string devicePhone { get; set; }
        public string deviceTwitter { get; set; }
        public string deviceFacebook { get; set; }
        public Nullable<System.DateTime> deviceLastAccessed { get; set; }
        public bool deviceActive { get; set; }
        public Nullable<int> Language_id { get; set; }
        public int BadgeNumber { get; set; }
        public System.Guid DeviceGuid { get; set; }
        public string first_name { get; set; }
        public string last_name { get; set; }
        public string deviceNotes { get; set; }
        public bool EmailIsAuthed { get; set; }
        public bool PhoneIsAuthed { get; set; }
        public bool Confirmed13 { get; set; }
        public string CountryCode { get; set; }
        public string AuthToken { get; set; }
        public int SendFailedCount { get; set; }
        public string NHRegToken { get; set; }
        public bool AcceptTermsAndPrivacy { get; set; }
        public byte APIVersion { get; set; }
    }
}
