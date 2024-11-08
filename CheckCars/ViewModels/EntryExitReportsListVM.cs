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
        // Implementación de INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        // Método para notificar cambios en las propiedades
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public ICommand AddReport { get; } = new Command(async () =>
        await Application.Current.MainPage.Navigation.PushAsync(new AddEntryExitReport(), true));

        public ICommand ViewReport { get; } = new Command(async (e) =>
        {
            if (e is int reportId) // Cambia 'int' por el tipo adecuado si es necesario
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
                if (_EntryExitReports != value)
                {
                    _EntryExitReports = value;
                    OnPropertyChanged(nameof(EntryExitReports));
                }

            }
        }


        public ICommand UpdateReports { get; }
        public EntryExitReportsListVM()
        {
           
           LoadReports();
            
            UpdateReports = new Command( () => LoadReports());
        }


  
        public void LoadReports()
        {
     
                EntryExitReports = new ObservableCollection<EntryExitReport>(); 
                
                using (var db = new ReportsDBContextSQLite())
                {
                    var d = db.EntryExitReports.ToList();
                    EntryExitReports = new ObservableCollection<EntryExitReport>(d);
                }
     
          
        }

    }
}
