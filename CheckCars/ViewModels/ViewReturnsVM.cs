using CheckCars.Data;
using CheckCars.Models;
using CheckCars.Services;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace CheckCars.ViewModels
{
    /// <summary>
    /// ViewModel for displaying a vehicle return report.
    /// </summary>
    public class ViewReturnsVM : INotifyPropertyChangedAbst
    {
        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="ViewReturnsVM"/> class.
        /// Loads the vehicle return report from the local database based on the report ID.
        /// </summary>
        public ViewReturnsVM()
        {
            var Id = Data.StaticData.ReportId;
            _apiService = new APIService();

            ShareImage = new Command<string>(SharePhotoAsync);

            using (var db = new ReportsDBContextSQLite())
            {
                Report = db.Returns
                    .Include(e => e.Photos)
                    .FirstOrDefault(e => e.ReportId == Id);
            }
        }

        #endregion

        #region Properties

        /// <summary>
        /// Service to handle API communication.
        /// </summary>
        private readonly APIService _apiService;

        private VehicleReturn _Report = new();

        /// <summary>
        /// The vehicle return report loaded from the database.
        /// </summary>
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
        // Define any ICommand properties here for UI interactions (e.g., Send, Delete, Share).
      
        public ICommand ShareImage { get; }
        #endregion

        #region Methods
        // Define logic for interacting with the Report, such as uploading, downloading, or deleting.
       
        /// <summary>
        /// Shares a photo by file path using the Share API.
        /// </summary>
        /// <param name="obj">File path of the image to share.</param>
        private void SharePhotoAsync(string obj)
        {
            ShareFile(obj);
        }

        /// <summary>
        /// Shares a file using the native share feature.
        /// </summary>
        private async Task ShareFile(string filePath)
        {
            try
            {
                var request = new ShareFileRequest
                {
                    Title = "Compartir Imagen",
                    File = new ShareFile(filePath)
                };

                await Share.Default.RequestAsync(request);
            }
            catch (Exception d)
            {
                Application.Current.MainPage.DisplayAlert("Error", d.Message, "OK");
            }
        }

        #endregion
    }
}
