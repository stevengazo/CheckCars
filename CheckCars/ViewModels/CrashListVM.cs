using CheckCars.Data;
using CheckCars.Models;
using CheckCars.Views;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace CheckCars.ViewModels
{
    /// <summary>
    /// ViewModel that manages the collection of crash reports.
    /// Supports loading, viewing, and adding crash reports.
    /// Implements property change notification.
    /// </summary>
    public class CrashListVM : INotifyPropertyChangedAbst
    {
        #region Properties

        private ObservableCollection<CrashReport> _crashReports = new();

        /// <summary>
        /// Collection of crash reports for data binding.
        /// </summary>
        public ObservableCollection<CrashReport> CrashReports
        {
            get => _crashReports;
            set
            {
                _crashReports = value;
                if (_crashReports != null)
                {
                    OnPropertyChanged(nameof(CrashReports));
                }
            }
        }

        #endregion

        #region Commands

        /// <summary>
        /// Command to navigate to the AddCrash page for creating a new crash report.
        /// </summary>
        public ICommand AddCrashReport { get; } = new Command(async () => await Application.Current.MainPage.Navigation.PushAsync(new AddCrash()));

        /// <summary>
        /// Command to view a specific crash report by its ID.
        /// Navigates to the ViewCrash page.
        /// </summary>
        public ICommand ViewReport { get; } = new Command(async (e) =>
        {
            if (e is string reportId) // Adjust type if needed
            {
                Data.StaticData.ReportId = reportId;
                await Application.Current.MainPage.Navigation.PushAsync(new ViewCrash(), true);
            }
        });

        /// <summary>
        /// Command to refresh the crash reports collection by loading data asynchronously.
        /// </summary>
        public ICommand Update => new Command(() => LoadDataAsync());

        #endregion

        #region Methods

        /// <summary>
        /// Asynchronously loads the crash reports from the local database,
        /// orders them by creation date descending, and updates the observable collection.
        /// </summary>
        public async Task LoadDataAsync()
        {
            try
            {
                using (var db = new ReportsDBContextSQLite())
                {
                    CrashReports.Clear();
                    var data = db.CrashReports.OrderByDescending(e => e.Created).ToList();
                    foreach (var entry in data)
                    {
                        CrashReports.Add(entry);
                    }
                }
            }
            catch (Exception e)
            {
                await Application.Current.MainPage.DisplayAlert("Error", "No se pudo cargar la lista de informes de accidentes.", "OK");
                // You may log or handle the exception here if needed
                Console.WriteLine(e.Message);
            }
        }

        #endregion
    }
}
