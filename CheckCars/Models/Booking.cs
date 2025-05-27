using System.ComponentModel.DataAnnotations;

namespace CheckCars.Models
{
    /// <summary>
    /// Represents a booking for a vehicle.
    /// </summary>
    public class Booking
    {
        /// <summary>
        /// Gets or sets the unique identifier for the booking.
        /// </summary>
        [Key]
        public int BookingId { get; set; }

        /// <summary>
        /// Gets or sets the start date of the booking.
        /// </summary>
        public DateTime Startdate { get; set; }

        /// <summary>
        /// Gets or sets the end date of the booking.
        /// </summary>
        public DateTime EndDate { get; set; }

        /// <summary>
        /// Gets or sets the reason for the booking.
        /// </summary>
        public string Reason { get; set; } = "";

        /// <summary>
        /// Gets or sets the current status of the booking.
        /// </summary>
        public string Status { get; set; } = "";

        /// <summary>
        /// Gets or sets the user ID who made the booking.
        /// </summary>
        public string UserId { get; set; } = "";

        /// <summary>
        /// Gets or sets the province associated with the booking.
        /// </summary>
        public string Province { get; set; } = "";

        /// <summary>
        /// Gets or sets a value indicating whether the booking has been deleted.
        /// </summary>
        public bool Deleted { get; set; } = false;

        /// <summary>
        /// Gets or sets the ID of the car associated with the booking.
        /// </summary>
        public string CarId { get; set; }

        /// <summary>
        /// Gets or sets the car model related to this booking.
        /// </summary>
        public CarModel? Car { get; set; }
    }
}
