using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchoopFunctionApp.Entities
{
    public partial class tbl_active_years_by_device
    {
        public int activeyear_id { get; set; }
        public int device_id { get; set; }
        public int schoop_id { get; set; }
        public string active_years { get; set; }
    }
}
