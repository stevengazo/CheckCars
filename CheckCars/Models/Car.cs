using System.ComponentModel.DataAnnotations;

namespace CheckCars.Models
{
    /// <summary>
    /// Represents a car with its details.
    /// </summary>
    public class CarModel
    {
        /// <summary>
        /// Gets or sets the unique identifier for the car.
        /// Defaults to a new GUID as a string.
        /// </summary>
        [Key]
        public string CarId { get; set; } = Guid.NewGuid().ToString();

        /// <summary>
        /// Gets or sets the model name of the car.
        /// </summary>
        public string Model { get; set; }

        /// <summary>
        /// Gets or sets the brand or manufacturer of the car.
        /// </summary>
        public string Brand { get; set; }

        /// <summary>
        /// Gets or sets the license plate number of the car.
        /// </summary>
        public string Plate { get; set; }

        /// <summary>
        /// Gets or sets the acquisition date of the car.
        /// Defaults to today's date.
        /// </summary>
        public DateTime AdquisitionDate { get; set; } = DateTime.Today;

        /// <summary>
        /// Gets or sets the type of the car (e.g., sedan, SUV).
        /// </summary>
        public string? Type { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the fuel type of the car (e.g., gasoline, diesel).
        /// </summary>
        public string? FuelType { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the Vehicle Identification Number (VIN).
        /// </summary>
        public string? VIN { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the color of the car.
        /// </summary>
        public string? Color { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the width of the car.
        /// </summary>
        public double? Width { get; set; } = 0;

        /// <summary>
        /// Gets or sets the height of the car.
        /// </summary>
        public double? Height { get; set; } = 0;

        /// <summary>
        /// Gets or sets the length of the car.
        /// </summary>
        public double? Lenght { get; set; } = 0;

        /// <summary>
        /// Gets or sets the weight of the car.
        /// </summary>
        public double? Weight { get; set; } = 0;

        /// <summary>
        /// Gets or sets any additional notes about the car.
        /// </summary>
        public string? Notes { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the manufacturing year of the car.
        /// Defaults to the current year.
        /// </summary>
        public int? Year = DateTime.Today.Year;
    }
}
