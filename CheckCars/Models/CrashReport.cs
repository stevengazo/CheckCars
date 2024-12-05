namespace CheckCars.Models
{
    public class CrashReport : Report
    {

        public DateTime DateOfCrash { get; set; }
        public string CrashDetails { get; set; }
        public string Location { get; set; }
        public string CrashedParts { get; set; }
    }
}
