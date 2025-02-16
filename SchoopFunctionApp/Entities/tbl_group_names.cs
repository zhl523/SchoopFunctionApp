using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchoopFunctionApp.Entities
{
    public partial class tbl_group_names
    {
        public int nameID { get; set; }
        public int groupID { get; set; }
        public int languID { get; set; }
        public string groupName { get; set; }
        public string groupDescription { get; set; }
    }
}
