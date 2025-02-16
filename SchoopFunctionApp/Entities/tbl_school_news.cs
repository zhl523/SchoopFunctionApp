using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchoopFunctionApp.Entities
{
    public partial class tbl_school_news
    {
        public int school_news_id { get; set; }
        public int schoopID { get; set; }
        public System.DateTime school_news_date { get; set; }
        public string school_news_headline { get; set; }
        public string school_news_article { get; set; }
        public string school_news_active_years { get; set; }
        public bool school_news_active { get; set; }
        public int admin_id { get; set; }
        public Nullable<int> Language_id { get; set; }
        public Nullable<bool> Published { get; set; }
        public string rssGuid { get; set; }
        public string ActiveGroups { get; set; }
        public bool IsLoginAs { get; set; }
        public bool IsTranslatedByGoogle { get; set; }
        public bool Deleted { get; set; }
        public bool IsPrivate { get; set; }
        public bool SendByDeviceIds { get; set; }
        public string DeviceIds { get; set; }
    }
}
