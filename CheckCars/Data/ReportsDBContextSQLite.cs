using CheckCars.Models;
using Microsoft.EntityFrameworkCore;

namespace CheckCars.Data
{
    public class ReportsDBContextSQLite : DbContext
    {
        public DbSet<CarModel> Cars { get; set; }
        public DbSet<CrashReport> CrashReports { get; set; }
        public DbSet<EntryExitReport> EntryExitReports { get; set; }
        public DbSet<IssueReport> IssueReports { get; set; }
        public DbSet<Photo> Photos { get; set; }
        public DbSet<Report> Reports { get; set; }

        public ReportsDBContextSQLite(DbContextOptions<ReportsDBContextSQLite> contextOptions)
        {
            Database.EnsureCreated();
        }
        public ReportsDBContextSQLite()
        {

        }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite($"Filename={GetPath($"{nameof(ReportsDBContextSQLite)}.db3")}");
            SQLitePCL.Batteries_V2.Init();
            // https://sagbansal.medium.com/how-to-update-the-sqlite-database-after-each-app-release-using-entity-framework-xamarin-maui-7b582313f89

        }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

        }


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

    }
}
