namespace CheckCars.Models
{
    /// <summary>
    /// Represents a user's profile information.
    /// </summary>
    public class UserProfile
    {
        /// <summary>
        /// Gets or sets the user's username.
        /// </summary>
        public string? UserName { get; set; }

        /// <summary>
        /// Gets or sets the user's DNI (identification number).
        /// </summary>
        public int DNI { get; set; }
    }
}
