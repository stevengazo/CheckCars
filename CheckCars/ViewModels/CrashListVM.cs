using Microsoft.EntityFrameworkCore;
using ReviCar.Data;
using ReviCar.Models;
using ReviCar.Utilities;
using ReviCar.Views;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace ReviCar.ViewModels
{
    /// <summary>
    /// ViewModel that manages the collection of crash reports.
    /// Supports loading, viewing, and adding crash reports.
    /// Implements property change notification.
    /// </summary>
    public class CrashListVM : INotifyPropertyChangedAbst
    {
        #region Constructor
        public CrashListVM()
        {
            Update = new Command(async () => await LoadDataAsync());
            AddCrashReport = new Command(async () => await Application.Current.MainPage.Navigation.PushAsync(new AddCrash()));
            ViewReport = new Command(async (e) =>
                 {
                     if (e is string reportId) // Adjust type if needed
                     {
                         Data.StaticData.ReportId = reportId;
                         await Application.Current.MainPage.Navigation.PushAsync(new ViewCrash(), true);
                     }
                 });
        }
        #endregion


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
        public ICommand AddCrashReport { get; }

        /// <summary>
        /// Command to view a specific crash report by its ID.
        /// Navigates to the ViewCrash page.
        /// </summary>
        public ICommand ViewReport { get; }

        /// <summary>
        /// Command to refresh the crash reports collection by loading data asynchronously.
        /// </summary>
        public ICommand Update { get; }

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
                    var data = await db.CrashReports.OrderByDescending(e => e.Created).ToListAsync();
                    foreach (var entry in data)
                    {
                        CrashReports.Add(entry);
                    }
                }
            }
            catch (Exception e)
            {
                await MessageUtilities.ShowLongToast("Error al cargar los informes de accidentes. Error: " + e.Message);
            }
        }

        #endregion
    }
}
