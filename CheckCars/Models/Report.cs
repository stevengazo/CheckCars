using System.ComponentModel.DataAnnotations;

namespace CheckCars.Models
{
    /// <summary>
    /// Abstract base class representing a generic report.
    /// </summary>
    public abstract class Report
    {
        /// <summary>
        /// Gets or sets the unique identifier for the report.
        /// </summary>
        [Key]
        public string ReportId { get; set; } = Guid.NewGuid().ToString();

        /// <summary>
        /// Gets or sets the author of the report.
        /// </summary>
        public string? Author { get; set; }

        /// <summary>
        /// Gets or sets the date and time when the report was created.
        /// </summary>
        public DateTime Created { get; set; }

        /// <summary>
        /// Gets or sets the car plate associated with the report.
        /// </summary>
        public string? CarPlate { get; set; }

        /// <summary>
        /// Gets or sets additional notes related to the report.
        /// </summary>
        public string? Notes { get; set; } = "";

        /// <summary>
        /// Gets or sets the latitude coordinate where the report was created.
        /// </summary>
        public double Latitude { get; set; }

        /// <summary>
        /// Gets or sets the longitude coordinate where the report was created.
        /// </summary>
        public double Longitude { get; set; }

        /// <summary>
        /// Indicates whether the report has been uploaded.
        /// </summary>
        public bool isUploaded { get; set; } = false;

        /// <summary>
        /// Collection of photos related to the report.
        /// </summary>
        public ICollection<Photo>? Photos { get; set; }
    }
}
