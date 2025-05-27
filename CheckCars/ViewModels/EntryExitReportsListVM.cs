using CheckCars.Data;
using CheckCars.Models;
using CheckCars.Views;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace CheckCars.ViewModels
{
    /// <summary>
    /// ViewModel for managing a list of Entry/Exit reports.
    /// Supports loading, viewing, and adding reports.
    /// Implements property change notifications.
    /// </summary>
    public class EntryExitReportsListVM : INotifyPropertyChangedAbst
    {
        #region Constructor

        /// <summary>
        /// Initializes a new instance of <see cref="EntryExitReportsListVM"/> and loads the reports.
        /// </summary>
        public EntryExitReportsListVM()
        {
            LoadReports();
        }

        #endregion

        #region Properties

        private ObservableCollection<EntryExitReport> _EntryExitReports = new();

        /// <summary>
        /// Collection of Entry/Exit reports for data binding.
        /// </summary>
        public ObservableCollection<EntryExitReport> EntryExitReports
        {
            get => _EntryExitReports;
            set
            {
                _EntryExitReports = value;
                if (_EntryExitReports != null)
                {
                    OnPropertyChanged(nameof(EntryExitReports));
                }
            }
        }

        #endregion

        #region Commands

        /// <summary>
        /// Command to navigate to the AddEntryExitReport page for adding a new report.
        /// </summary>
        public ICommand AddReport { get; } = new Command(async () => await Application.Current.MainPage.Navigation.PushAsync(new AddEntryExitReport(), true));

        /// <summary>
        /// Command to view an existing report by its ID.
        /// Navigates to the ViewEntryExit page.
        /// </summary>
        public ICommand ViewReport { get; } = new Command(async (e) =>
        {
            if (e is string reportId) // Change type if needed
            {
                Data.StaticData.ReportId = reportId;
                await Application.Current.MainPage.Navigation.PushAsync(new ViewEntryExit(), true);
            }
        });

        /// <summary>
        /// Command to reload the list of reports.
        /// </summary>
        public ICommand UpdateReports => new Command(() => LoadReports());

        #endregion

        #region Methods

        /// <summary>
        /// Loads Entry/Exit reports from the local database asynchronously
        /// and populates the observable collection.
        /// </summary>
        public async Task LoadReports()
        {
            try
            {
                using (var db = new ReportsDBContextSQLite())
                {
                    EntryExitReports.Clear();
                    var data = db.EntryExitReports.OrderByDescending(e => e.Created).ToList();
                    foreach (var entry in data)
                    {
                        EntryExitReports.Add(entry);
                    }
                }
            }
            catch (Exception e)
            {
                await Application.Current.MainPage.DisplayAlert("Error", "Error al cargar los reportes", "OK");
                Console.WriteLine(e.Message);
            }
        }

        #endregion
    }
}
