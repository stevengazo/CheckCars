using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CheckCars.Models
{
    public class Photo
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public string PhotoId { get; set; } = Guid.NewGuid().ToString();
        public string FileName { get; set; }
        public string FilePath { get; set; }
        public DateTime DateTaken { get; set; }

        [ForeignKey("Report")]
        public string ReportId { get; set; }
        public Report Report { get; set; }

    }
}
