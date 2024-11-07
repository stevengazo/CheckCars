using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CheckCars.Models
{
    public class Photo
    {

        public int PhotoId { get; set; }
        public string FileName { get; set; }
        public string FilePath { get; set; }
        public DateTime DateTaken { get; set; }

        [ForeignKey("Report")]
        public int ReportId { get; set; }
        public Report Report { get; set; }

    }
}
