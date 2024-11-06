using CheckCars.Data;
using CheckCars.Models;
using CheckCars.Views;
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
    public class EntryExitReportsListVM :INotifyPropertyChangedAbst
    {
        // Implementación de INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        // Método para notificar cambios en las propiedades
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public ICommand AddReport { get; } = new Command(async () =>
        await Application.Current.MainPage.Navigation.PushAsync(new AddEntryExitReport(),true));

        public List<EntryExitReport> _EntryExitReports= new();
        
        public List<EntryExitReport> EntryExitReports
        {
            get { return _EntryExitReports; }
            set
            {
                if (_EntryExitReports != value)
                {
                    _EntryExitReports = value;
                    OnPropertyChanged(nameof(EntryExitReports));
                }

            }
        }

        public EntryExitReportsListVM()
        {
            using (var db = new ReportsDBContextSQLite())
            {
                EntryExitReports = db.EntryExitReports.ToList();
            }
        }
    }
}
