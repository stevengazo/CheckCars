using CheckCars.Data;
using CheckCars.Models;
using CheckCars.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CheckCars.ViewModels
{
    public class AddBookingVM : INotifyPropertyChangedAbst
    {
        #region Constructor
        public AddBookingVM()
        {
            // booking. =  Preferences.Get(nameof(UserProfile.UserName), "Nombre de Usuario");
            carsList = _db.Cars.Select(e => e.Plate).ToList();
        }
        #endregion

        #region Properties
        private readonly APIService aPIService = new APIService();
        private readonly ReportsDBContextSQLite _db = new();

        private List<string> carsList = new();
        public List<string> CarsList
        {
            get { return carsList; }
            set
            {
                if (carsList != value)
                {
                    carsList = value;
                    OnPropertyChanged();
                }
            }
        }

        private Booking booking { get; set; } = new Booking()
        {
            Startdate = DateTime.Now,
            EndDate = DateTime.Now.AddHours(1),
        };
        public Booking Booking
        {
            get { return booking; }
            set
            {
                if (booking != value)
                {
                    booking = value;
                    OnPropertyChanged();
                }
            }
        }

        private string selectedCar { get; set; } = string.Empty;
        public string SelectedCar
        {
            get { return selectedCar; }
            set
            {
                if (selectedCar != value)
                {
                    booking.CarId = _db.Cars.FirstOrDefault(e => e.Plate == value).CarId;
                    selectedCar = value;
                    OnPropertyChanged();
                }
            }
        }
        private DateTime _startDateTime = DateTime.Now;
        public DateTime StartDate
        {
            get => _startDateTime;
            set
            {
                if (_startDateTime != value)
                {
                    _startDateTime = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(StartDatePart));
                    OnPropertyChanged(nameof(StartTimePart));
                }
            }
        }
        public DateTime StartDatePart
        {
            get => _startDateTime.Date;
            set
            {
                StartDate = value.Date + _startDateTime.TimeOfDay;
            }
        }
        public TimeSpan StartTimePart
        {
            get => _startDateTime.TimeOfDay;
            set
            {
                StartDate = _startDateTime.Date + value;
            }
        }
        private DateTime _endDateTime = DateTime.Now.AddHours(1);
        public DateTime EndDate
        {
            get => _endDateTime;
            set
            {
                if (value < StartDate)
                {
                    Application.Current.MainPage.DisplayAlert("Error", "La fecha final no puede ser anterior a la fecha de inicio", "OK");
                    return;
                }

                if (_endDateTime != value)
                {
                    _endDateTime = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(EndDatePart));
                    OnPropertyChanged(nameof(EndTimePart));
                }
            }
        }
        public DateTime EndDatePart
        {
            get => _endDateTime.Date;
            set
            {
                EndDate = value.Date + _endDateTime.TimeOfDay;
            }
        }
        public TimeSpan EndTimePart
        {
            get => _endDateTime.TimeOfDay;
            set
            {
                EndDate = _endDateTime.Date + value;
            }
        }
        private string _ErrorMessage { get; set; } = string.Empty;
        public string ErrorMessage
        {
            get { return _ErrorMessage; }
            set
            {
                if (_ErrorMessage != value)
                {
                    _ErrorMessage = value;
                    OnPropertyChanged();
                }
            }
        }

        #endregion


        #region Commands

        public Command AddBookingCommand => new Command(async () => await AddBooking());
        public Command CancelBookingCommand => new Command(async () => await CancelBooking());




        #endregion

        #region Methods
        private async Task CheckAvariable()
        {
            try
            {
                var url = $"api/Bookings/Search?startDate={StartDate.ToString("yyyy-MM-ddTHH:mm:ss")}&endDate={EndDate.ToString("yyyy-MM-ddTHH:mm:ss")}";
                var bookings = await aPIService.GetAsync<List<Booking>>(url,TimeSpan.FromSeconds(24));
                if (!bookings.Any())
                {
                    booking.UserId = "6f969ce2-53a1-4b39-b8d0-aa0d25c5c4bb";
                    booking.Deleted = false;
                    bool add = await Application.Current.MainPage.DisplayAlert(url, "¿Desea añadir la reserva?", "Añadir reserva", "Cancelar");
                    if (add)
                    {
                        var R = await aPIService.PostAsync<Booking>("api/bookings", booking, TimeSpan.FromSeconds(23));
                        if (R)
                        {
                            await Application.Current.MainPage.DisplayAlert("info", "Reserva Añadida", "OK");
                        }
                    }

                    else
                    {
                        return;
                    }

                }
                else
                {
                    Application.Current.MainPage.DisplayAlert("Error", "Vehículo no disponible", "OK");
                }

            }
            catch (Exception e)
            {
                Application.Current.MainPage.DisplayAlert("Error", e.Message, "OK");
            }

        }
        private async Task AddBooking()
        {
            try
            {

                CheckAvariable();

            }
            catch (Exception ds)
            {
                Application.Current.MainPage.DisplayAlert("Error", ds.Message, "OK");
                throw;
            }
        }

        private async Task CancelBooking()
        {
            try
            {
                Application.Current.MainPage.Navigation.RemovePage(Application.Current.MainPage.Navigation.NavigationStack[Application.Current.MainPage.Navigation.NavigationStack.Count - 1]);
            }
            catch (Exception fd)
            {
                Application.Current.MainPage.DisplayAlert("Error", fd.Message, "OK");

            }
        }

        private async Task SendBooking()
        {
            try
            {

            }
            catch (Exception f)
            {
                Application.Current.MainPage.DisplayAlert("Error", f.Message, "OK");
            }
        }


        #endregion
    }
}
