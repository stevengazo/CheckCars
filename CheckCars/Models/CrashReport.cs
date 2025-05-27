namespace CheckCars.Models
{
    /// <summary>
    /// Represents a crash report, inheriting from the base Report class.
    /// Contains details specific to a vehicle crash event.
    /// </summary>
    public class CrashReport : Report
    {
        /// <summary>
        /// Gets or sets the date when the crash occurred.
        /// </summary>
        public DateTime DateOfCrash { get; set; }

        /// <summary>
        /// Gets or sets detailed information about the crash.
        /// </summary>
        public string? CrashDetails { get; set; }

        /// <summary>
        /// Gets or sets the location where the crash happened.
        /// </summary>
        public string? Location { get; set; }

        /// <summary>
        /// Gets or sets a description of the parts of the vehicle that were crashed.
        /// </summary>
        public string? CrashedParts { get; set; }
    }
}
