using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchoopFunctionApp.Entities
{
    public partial class tbl_active_groups_by_device
    {
        public int activegroup_id { get; set; }
        public int deviceID { get; set; }
        public int active_groupID { get; set; }
    }
}
