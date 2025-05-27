namespace CheckCars.Models
{
    /// <summary>
    /// Represents a vehicle entry and exit report, extending the base Report class.
    /// Includes various vehicle condition details and equipment statuses.
    /// </summary>
    public class EntryExitReport : Report
    {
        /// <summary>
        /// Gets or sets the vehicle mileage.
        /// </summary>
        public long mileage { get; set; }

        /// <summary>
        /// Gets or sets the fuel level.
        /// </summary>
        public double FuelLevel { get; set; }

        /// <summary>
        /// Gets or sets additional notes.
        /// </summary>
        public string Notes { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the vehicle has a USB charger.
        /// </summary>
        public bool HasChargerUSB { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the vehicle has QuickPass.
        /// </summary>
        public bool HasQuickPass { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the vehicle has a phone support mount.
        /// </summary>
        public bool HasPhoneSupport { get; set; }

        /// <summary>
        /// Gets or sets the state of the tires.
        /// </summary>
        public string? TiresState { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the vehicle has a spare tire.
        /// </summary>
        public bool HasSpareTire { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the vehicle has an emergency kit.
        /// </summary>
        public bool HasEmergencyKit { get; set; }

        /// <summary>
        /// Gets or sets the paint condition of the vehicle.
        /// </summary>
        public string? PaintState { get; set; }

        /// <summary>
        /// Gets or sets the mechanical condition of the vehicle.
        /// </summary>
        public string? MecanicState { get; set; }

        /// <summary>
        /// Gets or sets the oil level.
        /// </summary>
        public string? OilLevel { get; set; }

        /// <summary>
        /// Gets or sets the condition of the vehicle interiors.
        /// </summary>
        public string? InteriorsState { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the vehicle interior is clean.
        /// </summary>
        public bool InteriorIsClean { get; set; }
    }
}
