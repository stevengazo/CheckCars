using ReviCar.Data;
using ReviCar.Models;
using ReviCar.Views;
using System.Collections.ObjectModel;
using System.Windows.Input;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ReviCar.Utilities;  // For ToListAsync if using EF Core

namespace ReviCar.ViewModels
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

            AddIssueReport = new Command(async () => await Application.Current.MainPage.Navigation.PushAsync(new AddIssuesReport()));
            ViewIssue= new Command(async (e) =>
            {
                if (e is string reportId)
                {
                    Data.StaticData.ReportId = reportId;
                    await Application.Current.MainPage.Navigation.PushAsync(new ViewIssue(), true);
                }
            });
            UpdateIssues = new Command(async () => await LoadData());

            _= InitializedAsync();


        }

        private async Task InitializedAsync()
        {
            try
            {
             await LoadData();
            }
            catch (Exception v)
            {
                MessageUtilities.ShowLongToast("Error al inicializar la vista de informes de problemas: " + v.Message).Wait();
            }
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
        public ICommand AddIssueReport { get; } 
        /// <summary>
        /// Command to view the details of a specific issue report by its ID.
        /// </summary>
        public ICommand ViewIssue { get; } 

        /// <summary>
        /// Command to reload and update the list of issue reports.
        /// </summary>
        public ICommand UpdateIssues { get; }

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
                await MessageUtilities.ShowLongToast("Error al cargar los informes de problemas: " + e.Message);
            }
        }

        #endregion
    }
}
