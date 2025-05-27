using CheckCars.Models;
using Microsoft.EntityFrameworkCore;

namespace CheckCars.Data
{
    /// <summary>
    /// SQLite database context for Reports, Cars, and related entities.
    /// </summary>
    public class ReportsDBContextSQLite : DbContext
    {
        #region Properties  

        /// <summary>
        /// DbSet for cars.
        /// </summary>
        public DbSet<CarModel> Cars { get; set; }

        /// <summary>
        /// DbSet for crash reports.
        /// </summary>
        public DbSet<CrashReport> CrashReports { get; set; }

        /// <summary>
        /// DbSet for entry and exit reports.
        /// </summary>
        public DbSet<EntryExitReport> EntryExitReports { get; set; }

        /// <summary>
        /// DbSet for issue reports.
        /// </summary>
        public DbSet<IssueReport> IssueReports { get; set; }

        /// <summary>
        /// DbSet for photos.
        /// </summary>
        public DbSet<Photo> Photos { get; set; }

        /// <summary>
        /// DbSet for generic reports.
        /// </summary>
        public DbSet<Report> Reports { get; set; }

        /// <summary>
        /// DbSet for vehicle returns.
        /// </summary>
        public DbSet<VehicleReturn> Returns { get; set; }

        /// <summary>
        /// DbSet for bookings.
        /// </summary>
        public DbSet<Booking> Bookings { get; set; }

        /// <summary>
        /// Constructor with options, ensures database is created.
        /// </summary>
        /// <param name="contextOptions">Options for the DbContext.</param>
        public ReportsDBContextSQLite(DbContextOptions<ReportsDBContextSQLite> contextOptions)
        {
            Database.EnsureCreated();
        }
        #endregion

        #region Constructor
        /// <summary>
        /// Parameterless constructor for EF usage.
        /// </summary>
        public ReportsDBContextSQLite()
        {
        }
        #endregion

        #region Methods

        /// <summary>
        /// Configures the SQLite database connection and initializes SQLite native libraries.
        /// </summary>
        /// <param name="optionsBuilder">The options builder.</param>
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite($"Filename={GetPath($"{nameof(ReportsDBContextSQLite)}.db3")}");
            SQLitePCL.Batteries_V2.Init();

            // Reference: https://sagbansal.medium.com/how-to-update-the-sqlite-database-after-each-app-release-using-entity-framework-xamarin-maui-7b582313f89
        }

        /// <summary>
        /// Configures the EF model.
        /// </summary>
        /// <param name="modelBuilder">The model builder.</param>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            // Additional model configuration can be added here.
        }

        /// <summary>
        /// Gets the platform-specific path to the SQLite database file.
        /// </summary>
        /// <param name="nameDB">Database file name.</param>
        /// <returns>Full path for the database file.</returns>
        private static string GetPath(string nameDB)
        {
            string pathDbLite = string.Empty;

            if (DeviceInfo.Platform == DevicePlatform.Android)
            {
                pathDbLite = Path.Combine(FileSystem.AppDataDirectory, nameDB);
            }
            else if (DeviceInfo.Platform == DevicePlatform.iOS)
            {
                pathDbLite = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                pathDbLite = Path.Combine(pathDbLite, "..", "Library", nameDB);
            }
            return pathDbLite;
        }
       
        #endregion
    }
}
