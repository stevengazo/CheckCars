using System.ComponentModel.DataAnnotations;

namespace CheckCars.Models
{
    public class Booking
    {
        [Key]
        public int BookingId { get; set; }
        public DateTime Startdate { get; set; }
        public DateTime EndDate { get; set; }
        public string Reason { get; set; } = "";
        public string Status { get; set; } = "";
        public string UserId { get; set; } = "";
        public string Province { get; set; } = "";

        public bool Deleted { get; set; } = false;

        public string CarId { get; set; }
        public CarModel? Car { get; set; }
    }
}
