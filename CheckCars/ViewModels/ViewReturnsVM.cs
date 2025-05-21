using CheckCars.Data;
using CheckCars.Models;
using CheckCars.Services;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
  
namespace CheckCars.ViewModels
{
    public class ViewReturnsVM : INotifyPropertyChangedAbst
    {
        #region Constructor

        public ViewReturnsVM()
        {
            var Id = Data.StaticData.ReportId;
            _apiService = new APIService();

            using (var db = new ReportsDBContextSQLite())
            {
                Report = db.Returns
                    .Include(e=>e.Photos)
                    .FirstOrDefault(e => e.ReportId == Id);
            }
        }
        #endregion

        #region Properties
        private readonly APIService _apiService;

        private VehicleReturn _Report = new();
        public VehicleReturn Report
        {
            get { return _Report; }
            set
            {
                if (_Report != value)
                {
                    _Report = value;
                    OnPropertyChanged(nameof(Report));
                }
            }
        }
        #endregion

        #region Commands

        #endregion

        #region Methods

        #endregion



    }
}
