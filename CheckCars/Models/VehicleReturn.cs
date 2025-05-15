namespace CheckCars.Models
{
    public class VehicleReturn : Report
    {
        public long mileage { get; set; }
        /// <summary>
        /// Nivel de Aceite
        /// </summary>
        public string? Notes { get; set; }
    }
}
