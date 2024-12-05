namespace CheckCars.Models
{
    public class IssueReport : Report
    {
        public string? Details { get; set; }
        public string? Priority { get; set; }
        public string? Type { get; set; }

    }
}
