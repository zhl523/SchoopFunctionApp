using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchoopFunctionApp.Entities
{
    public class StickyForm
    {
        public int Survey_ID { get; set; }
        public string Survey_Name { get; set; }
        public Guid SurveyGuid { get; set; }
        public string Alert_text { get; set; }

    }
}
