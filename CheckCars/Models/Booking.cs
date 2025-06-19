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
        public DateTime StartDate { get; set; }

        /// <summary>
        /// Gets or sets the end date of the booking.
        /// </summary>
        public DateTime EndDate { get; set; }

        /// <summary>
        /// Gets or sets the reason for the booking.
        /// </summary>
        public string Reason { get; set; }

        /// <summary>
        /// Gets or sets the current status of the booking.
        /// </summary>
        public string Status { get; set; }

        /// <summary>
        /// Gets or sets the identifier of the user who made the booking.
        /// </summary>
        public string UserId { get; set; }

        /// <summary>
        /// Gets or sets the province related to the booking.
        /// </summary>
        public string Province { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the booking has been deleted.
        /// </summary>
        public bool Deleted { get; set; } = false;

        /// <summary>
        /// Gets or sets the identifier of the car being booked.
        /// </summary>
        public string CarId { get; set; }
        /// <summary>
        /// Gets or sets a value indicating whether the booking is confirmed.
        /// </summary>
        public bool Confirmed { get; set; } = false;

        /// <summary>
        /// Gets or sets the car associated with the booking.
        /// </summary>
        public CarModel? Car { get; set; }
    }
}
