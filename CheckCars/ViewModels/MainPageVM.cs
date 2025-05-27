using CheckCars.Data;
using CheckCars.Views;
using System.ComponentModel;
using System.Windows.Input;

namespace CheckCars.ViewModels
{
    /// <summary>
    /// ViewModel for the main page. Provides commands for navigating to different report views.
    /// </summary>
    public class MainPageVM : INotifyPropertyChanged
    {
        #region Commands

        /// <summary>
        /// Command to navigate to the Entry/Exit report list page.
        /// </summary>
        public ICommand ViewEntryExitList { get; } = new Command(async () => await Application.Current.MainPage.Navigation.PushAsync(new EntryExitReportList(), true));

        /// <summary>
        /// Command to navigate to the crash reports list page.
        /// </summary>
        public ICommand CrashList { get; } = new Command(async () => await Application.Current.MainPage.Navigation.PushAsync(new CrashList(), true));

        /// <summary>
        /// Command to navigate to the issues list page.
        /// </summary>
        public ICommand IssuesList { get; } = new Command(async () => await Application.Current.MainPage.Navigation.PushAsync(new IssuesList(), true));

        /// <summary>
        /// Command to navigate to the returns list page.
        /// </summary>
        public ICommand ReturnList { get; } = new Command(async () => await Application.Current.MainPage.Navigation.PushAsync(new ReturnsPage(), true));

        /// <summary>
        /// Test command for internal testing or debugging.
        /// </summary>
        public ICommand TestCommand { get; }

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="MainPageVM"/> class.
        /// </summary>
        public MainPageVM()
        {
            TestCommand = new Command(() => test());
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MainPageVM"/> class with a database context.
        /// </summary>
        /// <param name="db">The database context to use for data access.</param>
        public MainPageVM(ReportsDBContextSQLite db)
        {
            reportsDB = db;
            TestCommand = new Command(() => test());
        }

        #endregion

        #region Methods

        /// <summary>
        /// Test method to retrieve crash reports from the database.
        /// </summary>
        private void test()
        {
            var d = reportsDB.CrashReports.ToList();
        }

        /// <summary>
        /// Triggers the PropertyChanged event for a given property.
        /// </summary>
        /// <param name="propertyName">The name of the property that changed.</param>
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion

        #region Properties

        /// <summary>
        /// The local database context used to access report data.
        /// </summary>
        private readonly ReportsDBContextSQLite reportsDB;

        /// <summary>
        /// Event raised when a property value changes.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        #endregion
    }
}
