using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using CheckCars.Data;
using CheckCars.Models;
using CheckCars.Views;

namespace CheckCars.ViewModels
{
    /// <summary>
    /// ViewModel for managing the vehicle returns page.
    /// </summary>
    public class ReturnsPageVM : INotifyPropertyChangedAbst
    {
        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="ReturnsPageVM"/> class.
        /// Loads return reports on initialization.
        /// </summary>
        public ReturnsPageVM()
        {
            loadData();
        }

        #endregion

        #region Commands

        /// <summary>
        /// Command to navigate to the add return report page.
        /// </summary>
        public ICommand ViewAddReturn { get; } = new Command(async () =>
            await Application.Current.MainPage.Navigation.PushAsync(new AddReturn(), true));

        /// <summary>
        /// Command to navigate to the view return report page with the specified report ID.
        /// </summary>
        public ICommand ViewReport { get; } = new Command(async (e) =>
        {
            if (e is string reportId)
            {
                Data.StaticData.ReportId = reportId;
                await Application.Current.MainPage.Navigation.PushAsync(new ViewReturn(), true);
            }
        });


        #endregion

        #region Properties

        private ObservableCollection<VehicleReturn> _Returns = new();
        /// <summary>
        /// Gets or sets the collection of vehicle return reports.
        /// </summary>
        public ObservableCollection<VehicleReturn> Returns
        {
            get { return _Returns; }
            set
            {
                _Returns = value;
                if (_Returns != null)
                {
                    OnPropertyChanged(nameof(Returns));
                }
            }
        }
        #endregion

        #region Methods
        /// <summary>
        /// Loads return data synchronously when the ViewModel is initialized.
        /// </summary>
        private void loadData()
        {
            try
            {
                using (var db = new ReportsDBContextSQLite())
                {
                    var d = db.Returns.OrderByDescending(e => e.Created).ToList();
                    if (d.Any())
                    {
                        Returns.Clear();
                        foreach (var v in d)
                        {
                            Returns.Add(v);
                        }
                    }
                }
            }
            catch (Exception we)
            {
                Application.Current.MainPage.DisplayAlert("Error", "No se pudo cargar la información", "OK");
                throw;
            }
        }

        #endregion
    }
}
