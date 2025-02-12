using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchoopFunctionApp.Entities
{
    public partial class tbl_schools_uk
    {
        public int SchoopID { get; set; }
        public Nullable<System.Guid> SchoolGUID { get; set; }
        public Nullable<bool> ExpressedInterest { get; set; }
        public string EstablishmentName { get; set; }
        public Nullable<short> EstablishmentNumber { get; set; }
        public string Street { get; set; }
        public string Locality { get; set; }
        public string Address3 { get; set; }
        public string Town { get; set; }
        public string County { get; set; }
        public string Postcode { get; set; }
        public string Gender { get; set; }
        public Nullable<double> TelephoneSTD { get; set; }
        public Nullable<double> TelephoneNum { get; set; }
        public string HeadTitle { get; set; }
        public string HeadFirstName { get; set; }
        public string HeadLastName { get; set; }
        public string HeadEmail { get; set; }
        public string HeadContactNumber { get; set; }
        public string TypeOfEstablishment { get; set; }
        public Nullable<System.DateTime> LastChangedDate { get; set; }
        public string LA_Name { get; set; }
        public Nullable<short> LA_Code { get; set; }
        public Nullable<byte> StatutoryHighAge { get; set; }
        public Nullable<byte> StatutoryLowAge { get; set; }
        public Nullable<int> UKPRN { get; set; }
        public Nullable<int> URN { get; set; }
        public string WebsiteAddress { get; set; }
        public string OfficialSixthForm { get; set; }
        public string PhaseOfEducation { get; set; }
        public Nullable<short> SchoolCapacity { get; set; }
        public Nullable<byte> ASCHighestAge { get; set; }
        public Nullable<byte> ASCLowestAge { get; set; }
        public string AdministrativeWard { get; set; }
        public string GOR { get; set; }
        public string ParliamentaryConstituency { get; set; }
        public Nullable<double> Easting { get; set; }
        public Nullable<double> Northing { get; set; }
        public int town_id { get; set; }
        public Nullable<System.DateTime> ExpiryDate { get; set; }
        public bool IsActive { get; set; }
        public int PaymentMethodId { get; set; }
        public string language { get; set; }
        public Nullable<int> TotalAlertSent { get; set; }
        public Nullable<int> AlertSentFailed { get; set; }
        public Nullable<float> Longitude { get; set; }
        public Nullable<float> Latitude { get; set; }
        public Nullable<bool> hasYears { get; set; }
        public Nullable<bool> hasGroups { get; set; }
        public Nullable<byte> organisationType { get; set; }
        public Nullable<bool> IsAcademy { get; set; }
        public string Email { get; set; }
        public string Designation { get; set; }
        public Nullable<short> OfstedRating { get; set; }
        public Nullable<bool> ISTPS { get; set; }
        public Nullable<short> Pupils { get; set; }
        public Nullable<short> associateID { get; set; }
        public Nullable<byte> account_status { get; set; }
        public Nullable<int> mshId { get; set; }
        public string Currency { get; set; }
        public string CustomDateFormat { get; set; }
        public Nullable<short> TimeZone { get; set; }
        public string TimeZoneName { get; set; }
        public string CountryCode { get; set; }
        public string LogoUrl { get; set; }
        public int TranslateUsage { get; set; }
        public int DefaultLanguageID { get; set; }
        public bool AutoSetGoogleTranslate { get; set; }
        public string IPWhiteList { get; set; }
        public bool EnableIPAccess { get; set; }
        public Nullable<int> SchoolClusterID { get; set; }
        public string SchoolClusterName { get; set; }
        public bool HaveSentCreditRunUpEmail { get; set; }
        public bool EnableSMS { get; set; }
        public int SMSCredit { get; set; }
        public string TwilioID { get; set; }
        public string channelDescription { get; set; }
        public int OpenTimeHour { get; set; }
        public int OpenTimeMinute { get; set; }
        public int CloseTimeHour { get; set; }
        public int CloseTimeMinute { get; set; }
        public bool isSchool { get; set; }
        public bool DisplayChildNameInFirstColumn { get; set; }
        public bool HideSchoopID { get; set; }
    }
}
