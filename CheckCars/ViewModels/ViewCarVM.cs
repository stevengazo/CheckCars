using CheckCars.Data;
using CheckCars.Models;
using CheckCars.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CheckCars.ViewModels
{
    public class ViewCarVM : INotifyPropertyChangedAbst
    {

        #region Properties  
        private readonly APIService _apiService = new();

        #endregion

        #region Constructor
        public ViewCarVM() 
        {

    
            var id = StaticData.CarId;
            using (var db =  new ReportsDBContextSQLite ())
            {
                Vehicle = db.Cars.FirstOrDefault(e => e.CarId == id);
            }

            RequestExists();
            IssuesExists();
            ReturnsExists();


        }
        #endregion

        #region Properties

        private CarModel _Vehicle;

        public CarModel Vehicle
        {
            get { return _Vehicle; }
            set
            {
                if (_Vehicle != value)
                {
                    _Vehicle = value;
                    OnPropertyChanged(nameof(Vehicle));
                }
            }
        }

        private ObservableCollection<EntryExitReport> _ExistsReports = new() ;

        public ObservableCollection<EntryExitReport> ExistsReports
        {
            get { return _ExistsReports; }
            set
            {
                if (_ExistsReports != value)
                {
                    _ExistsReports = value;
                    OnPropertyChanged(nameof(ExistsReports));
                }
            }
        }

        private ObservableCollection<IssueReport> _IssuesReports = new();

        public ObservableCollection<IssueReport> IssuesReports
        {
            get { return _IssuesReports; }
            set
            {
                if (_IssuesReports != value)
                {
                    _IssuesReports = value;
                    OnPropertyChanged(nameof(IssuesReports));
                }
            }
        }

        private ObservableCollection<VehicleReturn> _ReturnReports = new();

        public ObservableCollection<VehicleReturn> ReturnReports
        {
            get { return _ReturnReports; }
            set
            {
                if (_ReturnReports != value)
                {
                    _ReturnReports = value;
                    OnPropertyChanged(nameof(ReturnReports));
                }
            }
        }


        #endregion

        #region Methods
        private async void RequestExists()
        {

            try
            {
                ExistsReports.Clear();

                // Today
                var info = await _apiService.GetAsync<List<EntryExitReport>>($"api/EntryExitReports/search?date={DateTime.Today.ToString("yyyy-MM-dd")}&carId={Vehicle.CarId}", TimeSpan.FromSeconds(30));
                if(info != null)
                {
                    foreach (var i in info)
                    {
                        i.Photos = await GetPhotos(i.ReportId);
                        ExistsReports.Add(i);
                    }
                }

                // Yesterday
                var dV = await _apiService.GetAsync<List<EntryExitReport>>($"api/EntryExitReports/search?date={DateTime.Today.AddDays(-1).ToString("yyyy-MM-dd")}&carId={Vehicle.CarId}", TimeSpan.FromSeconds(30));
               if(dV != null)
                {
                    foreach(var i in dV){
                        i.Photos = await GetPhotos(i.ReportId);
                        ExistsReports.Add(i);
                    }
                }                
            }
            catch (Exception ef)
            {
                Application.Current.MainPage.DisplayAlert("Error", "Error al cargar los reportes de entrada/salida", "OK");
                throw;
            }
        }

        private async void IssuesExists()
        {

            try
            {
                IssuesReports.Clear();

                // Today
                var info = await _apiService.GetAsync<List<IssueReport>>($"api/IssueReports/search?date={DateTime.Today.ToString("yyyy-MM-dd")}&carId={Vehicle.CarId}", TimeSpan.FromSeconds(30));
                if (info != null)
                {
                    foreach (var i in info)
                    {
                        i.Photos = await GetPhotos(i.ReportId);
                        IssuesReports.Add(i);
                    }
                }
                // Yesterday
                var dV = await _apiService.GetAsync<List<IssueReport>>($"api/IssueReports/search?date={DateTime.Today.AddDays(-1).ToString("yyyy-MM-dd")}&carId={Vehicle.CarId}", TimeSpan.FromSeconds(30));
                if (dV != null)
                {
                    foreach (var i in dV)
                    {
                        i.Photos = await GetPhotos(i.ReportId);
                        IssuesReports.Add(i);
                    }
                }
            }
            catch (Exception ef)
            {
                Application.Current.MainPage.DisplayAlert("error", "Error al cargar los reportes de problemas. " + ef.Message, "OK");  
                throw;
            }
        }

        private async Task< List<CheckCars.Models.Photo>> GetPhotos( string id)
        {
            var info = await _apiService.GetAsync<List<CheckCars.Models.Photo>>($"api/Photos/report/{id}", TimeSpan.FromSeconds(30));
            if(info == null)
            {
                return new();
            }
            else
            {
                return info.ToList();
            }
        }

        private async void ReturnsExists()
        {

            try
            {
                ReturnReports.Clear();

                // Today
                var info = await _apiService.GetAsync<List<VehicleReturn>>($"api/VehicleReturns/search?date={DateTime.Today.ToString("yyyy-MM-dd")}&carId={Vehicle.CarId}", TimeSpan.FromSeconds(30));
                if (info != null)
                {
                    foreach (var i in info)
                    {
                        ReturnReports.Add(i);
                    }
                }

                // Yesterday
                var dV = await _apiService.GetAsync<List<VehicleReturn>>($"api/VehicleReturns/search?date={DateTime.Today.AddDays(-1).ToString("yyyy-MM-dd")}&carId={Vehicle.CarId}", TimeSpan.FromSeconds(30));
                if (dV != null)
                {
                    foreach (var i in dV)
                    {
                       ReturnReports.Add(i);
                    }
                }
            }
            catch (Exception ef)
            {
                Application.Current.MainPage.DisplayAlert("error", "Error al cargar los reportes de devoluciones. " + ef.Message, "OK");

                throw;
            }
        }

        #endregion
    }
}
