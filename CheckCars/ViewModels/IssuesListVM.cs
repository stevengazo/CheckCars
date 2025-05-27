using CheckCars.Data;
using CheckCars.Models;
using CheckCars.Views;
using System.Collections.ObjectModel;
using System.Windows.Input;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;  // For ToListAsync if using EF Core

namespace CheckCars.ViewModels
{
    /// <summary>
    /// ViewModel for managing and displaying a list of issue reports.
    /// </summary>
    public class IssuesListVM : INotifyPropertyChangedAbst
    {
        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="IssuesListVM"/> class
        /// and loads the issue reports asynchronously.
        /// </summary>
        public IssuesListVM()
        {
            Task.Run(() => LoadData());
        }

        #endregion

        #region Properties

        private ObservableCollection<IssueReport> _Issues = new();

        /// <summary>
        /// Gets or sets the collection of issue reports.
        /// </summary>
        public ObservableCollection<IssueReport> Issues
        {
            get => _Issues;
            set
            {
                _Issues = value;
                OnPropertyChanged(nameof(Issues));
            }
        }

        #endregion

        #region Commands

        /// <summary>
        /// Command to navigate to the Add Issue Report page.
        /// </summary>
        public ICommand AddIssueReport { get; } = new Command(async () => await Application.Current.MainPage.Navigation.PushAsync(new AddIssuesReport()));

        /// <summary>
        /// Command to view the details of a specific issue report by its ID.
        /// </summary>
        public ICommand ViewIssue { get; } = new Command(async (e) =>
        {
            if (e is string reportId)
            {
                Data.StaticData.ReportId = reportId;
                await Application.Current.MainPage.Navigation.PushAsync(new ViewIssue(), true);
            }
        });

        /// <summary>
        /// Command to reload and update the list of issue reports.
        /// </summary>
        public ICommand UpdateIssues => new Command(async () => await LoadData());

        #endregion

        #region Methods

        /// <summary>
        /// Loads the issue reports from the local database asynchronously,
        /// ordered by creation date descending.
        /// </summary>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task LoadData()
        {
            try
            {
                using (var db = new ReportsDBContextSQLite())
                {
                    Issues.Clear();
                    var data = await db.IssueReports.OrderByDescending(e => e.Created).ToListAsync();
                    foreach (var entry in data)
                    {
                        Issues.Add(entry);
                    }
                }
            }
            catch (Exception e)
            {
                await Application.Current.MainPage.DisplayAlert("Error", "No se pudo cargar la lista de informes de problemas.", "OK");
                Console.WriteLine(e.Message);
            }
        }

        #endregion
    }
}
