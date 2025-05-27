namespace CheckCars.Models
{
    /// <summary>
    /// Represents an issue report, extending the base Report class.
    /// Contains details, priority, and type of the issue.
    /// </summary>
    public class IssueReport : Report
    {
        /// <summary>
        /// Gets or sets the detailed description of the issue.
        /// </summary>
        public string? Details { get; set; }

        /// <summary>
        /// Gets or sets the priority level of the issue.
        /// </summary>
        public string? Priority { get; set; }

        /// <summary>
        /// Gets or sets the type/category of the issue.
        /// </summary>
        public string? Type { get; set; }
    }
}
