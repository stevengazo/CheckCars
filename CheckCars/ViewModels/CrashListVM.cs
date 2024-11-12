using CheckCars.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CheckCars.Models;
using CheckCars.Views;
using System.Windows.Input;
using CheckCars.Data;

namespace CheckCars.ViewModels
{
    public class CrashListVM : INotifyPropertyChangedAbst
    {
        // Implementación de INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        
        // Método para notificar cambios en las propiedades
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public ICommand AddCrashReport { get; } = new Command(async () => await Application.Current.MainPage.Navigation.PushAsync(new AddCrash()));

        private ObservableCollection<CrashReport> _crashReports = new();

        public ObservableCollection<CrashReport> CrashReports 
        { 
            get
            {
                return _crashReports;
            } 
            set 
            {
                _crashReports = value;
                if ( _crashReports != null)
                {
                    OnPropertyChanged(nameof(CrashReports));
                }
            }
        }

        public ICommand ViewReport { get; } = new Command(async (e) =>
        {
            if (e is int reportId) // Cambia 'int' por el tipo adecuado si es necesario
            {
                Data.StaticData.ReportId = reportId;
                await Application.Current.MainPage.Navigation.PushAsync(new ViewCrash(), true);
            }
        });


        public ICommand Update => new Command(() => LoadData());
        public async Task LoadData()
        {
            try
            {
                using (var db = new ReportsDBContextSQLite())
                {
                    CrashReports.Clear();
                    var data = db.CrashReports.ToList();
                    foreach (var entry in data)
                    {
                        CrashReports.Add(entry);
                    }
                }
            }
            catch (Exception e)
            {
                // Puedes registrar o manejar la excepción aquí si es necesario
                Console.WriteLine(e.Message);
            }
        }
    }
}
