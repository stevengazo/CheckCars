using CheckCars.Data;
using CheckCars.Views;
using System.ComponentModel;
using System.Windows.Input;

namespace CheckCars.ViewModels
{
    public class MainPageVM : INotifyPropertyChanged
    {
        #region Commands
        public ICommand ViewEntryExitList { get; } = new Command(async () => await Application.Current.MainPage.Navigation.PushAsync(new EntryExitReportList(), true));
        public ICommand CrashList { get; } = new Command(async () => await Application.Current.MainPage.Navigation.PushAsync(new CrashList(), true));
        public ICommand IssuesList { get; } = new Command(async () => await Application.Current.MainPage.Navigation.PushAsync(new IssuesList(), true));
        public ICommand ReturnList { get; } = new Command(async () => await Application.Current.MainPage.Navigation.PushAsync(new ReturnsPage(), true));
        public ICommand TestCommand { get; }
        #endregion

        #region constructor
        public MainPageVM()
        {   // Inicialización del comando
            TestCommand = new Command(() => test());
        }

        public MainPageVM(ReportsDBContextSQLite db)
        {
            reportsDB = db;
            // Inicialización del comando
            TestCommand = new Command(() => test());
        }

        #endregion

        #region Methods
        private void test()
        {
            var d = reportsDB.CrashReports.ToList();
        }
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        #region Properties
        private readonly ReportsDBContextSQLite reportsDB;
        // Implementación de INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        #endregion
    }
}