using CheckCars.Data;
using CheckCars.Models;
using CheckCars.Views;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace CheckCars.ViewModels
{
    public class EntryExitReportsListVM : INotifyPropertyChangedAbst
    {
        public ICommand AddReport { get; } = new Command(async () =>
        await Application.Current.MainPage.Navigation.PushAsync(new AddEntryExitReport(), true));
        public ICommand ViewReport { get; } = new Command(async (e) =>
        {
            if (e is string reportId) // Cambia 'int' por el tipo adecuado si es necesario
            {
                Data.StaticData.ReportId = reportId;
                await Application.Current.MainPage.Navigation.PushAsync(new ViewEntryExit(), true);
            }
        });
        public ObservableCollection<EntryExitReport> _EntryExitReports = new();
        public ObservableCollection<EntryExitReport> EntryExitReports
        {
            get { return _EntryExitReports; }
            set
            {
                _EntryExitReports = value;
                if (_EntryExitReports != null)
                {
                    OnPropertyChanged(nameof(EntryExitReports));
                }

            }
        }
        public EntryExitReportsListVM()
        {
           
          LoadReports();

        }
        public ICommand UpdateReports => new Command( () =>  LoadReports());
        public async Task LoadReports()
        {
            try
            {
                using (var db = new ReportsDBContextSQLite())
                {
                    EntryExitReports.Clear();
                    var data = db.EntryExitReports.ToList();
                    foreach (var entry in data)
                    {
                        EntryExitReports.Add(entry);
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
