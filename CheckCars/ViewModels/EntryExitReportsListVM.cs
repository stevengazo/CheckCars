using CheckCars.Data;
using CheckCars.Models;
using CheckCars.Views;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace CheckCars.ViewModels
{
    public class EntryExitReportsListVM : INotifyPropertyChangedAbst
    {
        #region Constructor
        public EntryExitReportsListVM()
        {

            LoadReports();

        }
        #endregion

        #region Properties

        public ObservableCollection<EntryExitReport> _EntryExitReports = new();
        public ObservableCollection<EntryExitReport> EntryExitReports
        {
            get { return _EntryExitReports; }
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
        public ICommand AddReport { get; } = new Command(async () => await Application.Current.MainPage.Navigation.PushAsync(new AddEntryExitReport(), true));
        public ICommand ViewReport { get; } = new Command(async (e) =>
        {
            if (e is string reportId) // Cambia 'int' por el tipo adecuado si es necesario
            {
                Data.StaticData.ReportId = reportId;
                await Application.Current.MainPage.Navigation.PushAsync(new ViewEntryExit(), true);
            }
        });

        public ICommand UpdateReports => new Command(() => LoadReports());

        #endregion

        #region Methods
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
                Application.Current.MainPage.DisplayAlert("Error", "Error al cargar los reportes", "OK");
                Console.WriteLine(e.Message);
            }
        }
        #endregion

    }
}
