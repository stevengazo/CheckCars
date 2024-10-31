using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CheckCars.Models
{
    public  abstract class Report
    {
        [Key]
        public int Id { get; set; }
        public string Name { get; set; }    
        public string Description { get; set; }
        public string Author { get; set; }
        public DateTime Created { get; set; }
        public long mileage { get; set; }
        public string CarPlate { get; set; }
        public ICollection<Photo> Photos { get; set; }
    }
}
