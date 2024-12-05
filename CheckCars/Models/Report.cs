using System.ComponentModel.DataAnnotations;

namespace CheckCars.Models
{
    public abstract class Report
    {
        [Key]
        public string ReportId { get; set; } = Guid.NewGuid().ToString();
        public string? Author { get; set; }
        public DateTime Created { get; set; }
        public string? CarPlate { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public ICollection<Photo>? Photos { get; set; }
    }
}
