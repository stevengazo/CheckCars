using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace CheckCars.Models
{
    public class EntryExitReport : Report
    {
        public string  FuelLevel { get; set; }
        public string CarState { get; set; }
        public bool HasCharger {  get; set; }
        public bool HasQuickPass { get; set; }
        public string TiresState { get; set; }
        public bool SpareTire { get; set; }
        public bool HasTools { get; set; }
    }
}
