using CheckCars.Data;
using CheckCars.Models;
using CheckCars.Views;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace CheckCars.ViewModels
{
    public class IssuesListVM : INotifyPropertyChangedAbst
    {
        #region Constructor
        public IssuesListVM()
        {

        }
        #endregion
       
        #region Properties
        public ICommand AddIssueReport { get; } = new Command(async () => await Application.Current.MainPage.Navigation.PushAsync(new AddIssuesReport()));
        public ICommand ViewIssue { get; } = new Command(async (e) =>
        {
            if (e is string reportId) // Cambia 'int' por el tipo adecuado si es necesario
            {
                Data.StaticData.ReportId = reportId;
                await Application.Current.MainPage.Navigation.PushAsync(new ViewIssue(), true);
            }
        });
        public ICommand UpdateIssues => new Command(() => LoadData());

        #endregion

        #region Properties
        private ObservableCollection<IssueReport> _Issues = new();
        public ObservableCollection<IssueReport> Issues
        {
            get { return _Issues; }
            set
            {
                _Issues = value;
                if (_Issues != null)
                {
                    OnPropertyChanged(nameof(Issues));
                }
            }
        }
        #endregion

        #region Methods
        public async Task LoadData()
        {
            try
            {
                using (var db = new ReportsDBContextSQLite())
                {
                    Issues.Clear();
                    var data = db.IssueReports.OrderByDescending(e=>e.Created).ToList();
                    foreach (var entry in data)
                    {
                        Issues.Add(entry);
                    }
                }
            }
            catch (Exception e)
            {
                Application.Current.MainPage.DisplayAlert("Error", "No se pudo cargar la lista de informes de problemas.", "OK");
                // Puedes registrar o manejar la excepción aquí si es necesario
                Console.WriteLine(e.Message);
            }
        }
        #endregion
    }
}
