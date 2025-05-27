namespace CheckCars.Interfaces
{
    /// <summary>
    /// Interface to manage application settings related to API usage and connection.
    /// </summary>
    public interface ISettingsService
    {
        /// <summary>
        /// Gets or sets a value indicating whether the API should be used.
        /// </summary>
        bool UseAPI { get; set; }

        /// <summary>
        /// Gets or sets the base URL of the API.
        /// </summary>
        string APIURL { get; set; }

        /// <summary>
        /// Gets or sets the port number for the API connection.
        /// </summary>
        string APIPort { get; set; }
    }
}
