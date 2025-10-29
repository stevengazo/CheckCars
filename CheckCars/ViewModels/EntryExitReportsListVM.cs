using Microsoft.EntityFrameworkCore;
using CheckCar.Data;
using CheckCar.Models;
using CheckCar.Utilities;
using CheckCar.Views;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace CheckCar.ViewModels
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

            try
            {
                AddReport = new Command(async () => await Application.Current.MainPage.Navigation.PushAsync(new AddEntryExitReport(), true));
                ViewReport = new Command(async (e) =>
                {
                    if (e is string reportId) // Change type if needed
                    {
                        Data.StaticData.ReportId = reportId;
                        await Application.Current.MainPage.Navigation.PushAsync(new ViewEntryExit(), true);
                    }
                });
                UpdateReports = new Command(async () => await LoadReportsAsync());

                _ = InitializedAsyc();
            }
            catch (Exception f)
            {
                MessageUtilities.ShowInfoMessageAsync(MessageUtilities.TitleError, "Error: " + f.Message);
            }
        }

        private async Task InitializedAsyc()
        {
            try
            {
                await LoadReportsAsync();
            }
            catch (Exception f)
            {
                await MessageUtilities.ShowInfoMessageAsync("EntryExitReportsListVM", "Error: " + f.Message);
            }
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
        public ICommand AddReport { get; } 

        /// <summary>
        /// Command to view an existing report by its ID.
        /// Navigates to the ViewEntryExit page.
        /// </summary>
        public ICommand ViewReport { get; }

        /// <summary>
        /// Command to reload the list of reports.
        /// </summary>
        public ICommand UpdateReports { get; }

        #endregion

        #region Methods

        /// <summary>
        /// Loads Entry/Exit reports from the local database asynchronously
        /// and populates the observable collection.
        /// </summary>
        public async Task LoadReportsAsync()
        {
            try
            {
                using (var db = new ReportsDBContextSQLite())
                {
                    EntryExitReports.Clear();
                    var data =await db.EntryExitReports.OrderByDescending(e => e.Created).ToListAsync();
                    foreach (var entry in data)
                    {
                        EntryExitReports.Add(entry);
                    }
                }
            }
            catch (Exception e)
            {
                await MessageUtilities.ShowLongToast("Error al cargar los reportes. Error: " + e.Message);
            }
        }

        #endregion
    }
}
