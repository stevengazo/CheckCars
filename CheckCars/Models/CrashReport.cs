using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace CheckCars.Models
{
    public class CrashReport : Report
    {
        public DateTime DateOfCrash {  get; set; }
        public string CrashDetails { get; set; }
        public string Location { get; set; }
        public string CrashedParts { get; set; }
    }
}
