using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchoopFunctionApp.Entities
{
    public partial class tbl_organisation_groups
    {
        public int groupID { get; set; }
        public int schoopID { get; set; }
        public string activeYears { get; set; }
        public Nullable<System.DateTime> lastUpdatedOn { get; set; }
        public decimal displayOrder { get; set; }
        public int parentGroupID { get; set; }
        public bool IsClosed { get; set; }
        public bool Deleted { get; set; }
    }
}
