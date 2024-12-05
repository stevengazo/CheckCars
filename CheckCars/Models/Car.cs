using System.ComponentModel.DataAnnotations;

namespace CheckCars.Models
{
    public class CarModel
    {
        [Key]

        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string Model { get; set; }
        public string Brand { get; set; }
        public string Plate { get; set; }
    }
}
