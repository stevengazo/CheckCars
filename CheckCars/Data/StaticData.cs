using CheckCars.Models;

namespace CheckCars.Data
{
    /// <summary>
    /// Provides static access to application preferences such as ReportId, User, URL, etc.
    /// </summary>
    public static class StaticData
    {
        private const string ReportIdKey = "ReportId";
        private const string UserProfileKey = "UserProfile";
        private const string URLKey = "URL";
        private const string PortKey = "Port";
        private const string UseAPIKey = "UseAPI";
        private const string _CarId = "";

        /// <summary>
        /// Gets or sets the selected car ID in preferences.
        /// </summary>
        public static string CarId
        {
            get => Preferences.Get(_CarId, string.Empty);
            set => Preferences.Set(_CarId, value);
        }

        /// <summary>
        /// Gets or sets the current report ID used in the application.
        /// </summary>
        public static string ReportId
        {
            get => Preferences.Get(ReportIdKey, string.Empty);
            set => Preferences.Set(ReportIdKey, value);
        }

        /// <summary>
        /// Gets or sets the base URL for the API.
        /// </summary>
        public static string URL
        {
            get => Preferences.Get(URLKey, string.Empty);
            set => Preferences.Set(URLKey, value);
        }

        /// <summary>
        /// Gets or sets the port used by the API.
        /// </summary>
        public static string Port
        {
            get => Preferences.Get(PortKey, string.Empty);
            set => Preferences.Set(PortKey, value);
        }

        /// <summary>
        /// Gets or sets whether the API should be used.
        /// </summary>
        public static bool UseA
        {
            get => Preferences.Get(UseAPIKey, true);
            set => Preferences.Set(UseAPIKey, value);
        }

        /// <summary>
        /// Gets or sets the current user profile stored in preferences.
        /// The object is serialized/deserialized using System.Text.Json.
        /// </summary>
        public static UserProfile User
        {
            get
            {
                var json = Preferences.Get(UserProfileKey, string.Empty);
                return string.IsNullOrEmpty(json) ? null : System.Text.Json.JsonSerializer.Deserialize<UserProfile>(json);
            }
            set
            {
                var json = System.Text.Json.JsonSerializer.Serialize(value);
                Preferences.Set(UserProfileKey, json);
            }
        }
    }
}
