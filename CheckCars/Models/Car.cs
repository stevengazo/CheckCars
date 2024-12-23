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
        public DateTime AdquisitionDate { get; set; } = DateTime.Today;

        public string? Type { get; set; } = string.Empty;
        public string? FuelType { get; set; } =string.Empty;

        public string? VIN { get; set; } = string.Empty;
        public string? Color { get; set; } = string.Empty;
        public double? Width { get; set; } = 0;
        public double? Height { get; set; } = 0;
        public double? Lenght { get; set; } = 0;
        public double? Weight { get; set; }= 0;
        public string? Notes { get; set; } = string.Empty;
        public int? Year = DateTime.Today.Year;
    }
}
