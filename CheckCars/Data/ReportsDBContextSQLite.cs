using vehiculosmecsa.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Devices;
using Microsoft.Data.Sqlite;

namespace vehiculosmecsa.Data
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
    // Obtén la ruta donde deseas almacenar la base de datos
    string dbPath = GetPath($"{nameof(ReportsDBContextSQLite) }.db3");
            
    // Verifica si el archivo de la base de datos existe
    if (!File.Exists(dbPath))
    {
        // Si no existe, crea el archivo (esto también puede inicializar la base de datos si se usa un ORM como EF Core)
        // Aquí solo aseguramos que el archivo esté disponible.
        var connection = new SqliteConnection($"Filename={dbPath}");
        connection.Open();
        connection.Close();
    }

    // Configura el DbContext para usar SQLite
    optionsBuilder.UseSqlite($"Filename={dbPath}");

    // Inicializa SQLite
    SQLitePCL.Batteries_V2.Init();

    // Aquí podrías agregar cualquier lógica adicional que necesites, como migraciones automáticas si es necesario.
}

        public static string GetPath(string nameDB)
        {
            string pathDbLite = string.Empty;


            if(DeviceInfo.Platform == DevicePlatform.Android)
            {
                pathDbLite = Path.Combine(FileSystem.AppDataDirectory, nameDB);  
            }else if(DeviceInfo.Platform == DevicePlatform.iOS)
            {
                pathDbLite = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                pathDbLite = Path.Combine( pathDbLite, "..","Library",nameDB);
            }
            return pathDbLite;
        }

    }
}
