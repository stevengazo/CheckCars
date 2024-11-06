using CheckCars.Data;
using CheckCars.Views;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace CheckCars.ViewModels
{
    public class MainPageVM : INotifyPropertyChanged
    {
        // Implementación de INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        // Comandos inicializados directamente
        public ICommand ViewEntryExitList { get; } = new Command(async () =>
            await Application.Current.MainPage.Navigation.PushAsync(new EntryExitReportList(), true));

        public ICommand CrashList { get; } = new Command(async () =>
        
        await Application.Current.MainPage.Navigation.PushAsync(new CrashList(), true));
        public ICommand IssuesList { get; } = new Command(async () =>
        await Application.Current.MainPage.Navigation.PushAsync(new IssuesList(), true));

        public ICommand TestCommand { get; }


        public MainPageVM()
        {   // Inicialización del comando
            TestCommand = new Command(() => test());
        }
        private readonly ReportsDBContextSQLite reportsDB;
        public MainPageVM(ReportsDBContextSQLite db)
        {
            reportsDB = db;
            // Inicialización del comando
            TestCommand = new Command(() => test());
        }

        private void test()
        {
            var d = reportsDB.CrashReports.ToList();    
        }



        // Método para notificar cambios en las propiedades
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}