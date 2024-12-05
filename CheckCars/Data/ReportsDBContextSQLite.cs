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

            modelBuilder.Entity<CarModel>().HasData(new List<CarModel>()
    {
        new CarModel() { Id = Guid.NewGuid().ToString(), Brand = "Daihatsu", Model = "Bego", Plate = "BKS-967" },
        new CarModel() { Id = Guid.NewGuid().ToString(), Brand = "SUZUKI", Model = "APV", Plate = "CL342882" },
        new CarModel() { Id = Guid.NewGuid().ToString(), Brand = "SUZUKI", Model = "CELERIO", Plate = "BQW-213" },
        new CarModel() { Id = Guid.NewGuid().ToString(), Brand = "TOYOTA", Model = "Raize", Plate = "BYG-089" },
        new CarModel() { Id = Guid.NewGuid().ToString(), Brand = "TOYOTA", Model = "Rush", Plate = "BYG-096" },
        new CarModel() { Id = Guid.NewGuid().ToString(), Brand = "TOYOTA", Model = "CROSS", Plate = "BZT-126" },
        new CarModel() { Id = Guid.NewGuid().ToString(), Brand = "JMC", Model = "Camión #1", Plate = "CL329127" },
        new CarModel() { Id = Guid.NewGuid().ToString(), Brand = "JMC", Model = "Camión #2", Plate = "CL328971" },
        new CarModel() { Id = Guid.NewGuid().ToString(), Brand = "Mitsubishi", Model = "Fuso Seal-Mec #1", Plate = "CL347751" },
        new CarModel() { Id = Guid.NewGuid().ToString(), Brand = "Mitsubishi", Model = "Fuso Asfaltos #2", Plate = "CL347703" },
        new CarModel() { Id = Guid.NewGuid().ToString(), Brand = "NISSAN", Model = "Frontier 4x4 #1 2008", Plate = "CL226304" },
        new CarModel() { Id = Guid.NewGuid().ToString(), Brand = "NISSAN", Model = "Frontier 4x4 #2 2025", Plate = "AGV 352" },
        new CarModel() { Id = Guid.NewGuid().ToString(), Brand = "NISSAN", Model = "Frontier 4x2 #3 2008", Plate = "CL212222" },
    });
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
