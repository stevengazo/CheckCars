namespace CheckCars.Models
{
    public class EntryExitReport : Report
    {
        public long mileage { get; set; }
        /// <summary>
        /// Nivel de Aceite
        /// </summary>
        public double FuelLevel { get; set; }
        /// <summary>
        ///  Notas
        /// </summary>
        public string Notes { get; set; }
        /// <summary>
        /// Cargador USB
        /// </summary>
        public bool HasChargerUSB { get; set; }
        /// <summary>
        /// Tiene Quickpass
        /// </summary>
        public bool HasQuickPass { get; set; }
        /// <summary>
        /// Tiene Soporte para telefono
        /// </summary>
        public bool HasPhoneSupport { get; set; }
        /// <summary>
        /// Estado de las llantas
        /// </summary>
        public string? TiresState { get; set; }
        /// <summary>
        /// Tiene Llanta de Refaccion
        /// </summary>
        public bool HasSpareTire { get; set; }
        /// <summary>
        /// Tiene kit de emergencia
        /// </summary>
        public bool HasEmergencyKit { get; set; }
        /// <summary>
        /// Estado de la pintura
        /// </summary>
        public string? PaintState { get; set; }
        /// <summary>
        /// Estado Mecanico
        /// </summary>
        public string? MecanicState { get; set; }
        /// <summary>
        /// Nivel de Aceite
        /// </summary>
        public string? OilLevel { get; set; }
        /// <summary>
        /// Estado de los interiores
        /// </summary>
        public string? InteriorsState { get; set; }
        public bool InteriorIsClean { get; set; }



    }
}
