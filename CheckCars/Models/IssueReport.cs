using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace vehiculosmecsa.Models
{
    public class IssueReport: Report
    {
        public string Details { get; set; }
        public string Priority { get; set; }
        public string Type { get; set; }

    }
}
