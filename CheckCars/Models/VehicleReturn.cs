namespace CheckCars.Models
{
    /// <summary>
    /// Represents a vehicle return report, inheriting from the base Report class.
    /// </summary>
    public class VehicleReturn : Report
    {
        /// <summary>
        /// Gets or sets the mileage of the vehicle at the time of return.
        /// </summary>
        public long mileage { get; set; }

        /// <summary>
        /// Gets or sets additional notes regarding the vehicle return.
        /// </summary>
        public string? Notes { get; set; }
    }
}
