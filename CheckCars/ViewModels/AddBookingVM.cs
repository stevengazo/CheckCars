using CheckCars.Data;
using CheckCars.Models;
using CheckCars.Services;
using iText.StyledXmlParser.Util;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CheckCars.ViewModels
{
    public class AddBookingVM : INotifyPropertyChangedAbst
    {
        #region Constructor
        /// <summary>
        /// Initializes a new instance of the AddBookingVM class,
        /// setting default booking dates and loading car plates.
        /// </summary>
        public AddBookingVM()
        {
            // booking. =  Preferences.Get(nameof(UserProfile.UserName), "Nombre de Usuario");
            booking = new Booking();
            booking.StartDate = DateTime.Now;
            booking.Status = "Pendiente";
            booking.Deleted = false;
            booking.Confirmed = false;
            booking.EndDate = DateTime.Now.AddHours(1);
            carsList = _db.Cars.Select(e => e.Plate).ToList();
            LoadingUsers();

        }
        #endregion

        #region Properties
        /// <summary>
        /// API service instance for network operations.
        /// </summary>
        private readonly APIService aPIService = new APIService();

        /// <summary>
        /// Database context instance for local data access.
        /// </summary>
        private readonly ReportsDBContextSQLite _db = new();

        /// <summary>
        /// List of available car plates.
        /// </summary>
        private List<string> carsList = new();

        /// <summary>
        /// Gets or sets the list of car plates.
        /// </summary>
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

        /// <summary>
        /// Booking object being created or edited.
        /// </summary>
        private Booking booking { get; set; }

        /// <summary>
        /// Gets or sets the booking details.
        /// </summary>
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

        /// <summary>
        /// Selected car plate string.
        /// </summary>
        private string selectedCar { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the selected car and updates the booking's CarId.
        /// </summary>
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

        /// <summary>
        /// Backing field for StartDate property.
        /// </summary>
        private DateTime _startDateTime = DateTime.Now;

        /// <summary>
        /// Gets or sets the start date and time of the booking.
        /// </summary>
        public DateTime StartDate
        {
            get => _startDateTime;
            set
            {
                if (_startDateTime != value)
                {
                    _startDateTime = value;
                    OnPropertyChanged(nameof(StartDate));
                    OnPropertyChanged(nameof(StartDatePart));
                    OnPropertyChanged(nameof(StartTimePart));
                }
            }
        }

        /// <summary>
        /// Gets or sets the date part of the start datetime.
        /// </summary>
        public DateTime StartDatePart
        {
            get => _startDateTime.Date;
            set
            {
                StartDate = value.Date + _startDateTime.TimeOfDay;
            }
        }

        /// <summary>
        /// Gets or sets the time part of the start datetime.
        /// </summary>
        public TimeSpan StartTimePart
        {
            get => _startDateTime.TimeOfDay;
            set
            {
                StartDate = _startDateTime.Date + value;
            }
        }

        /// <summary>
        /// Backing field for EndDate property.
        /// </summary>
        private DateTime _endDateTime = DateTime.Now.AddHours(1);

        /// <summary>
        /// Gets or sets the end date and time of the booking.
        /// Validates end date is not earlier than start date.
        /// </summary>
        public DateTime EndDate
        {
            get => _endDateTime;
            set
            {
                if (value < StartDate)
                {
                    Application.Current.MainPage.DisplayAlert("Error", "La fecha final no puede ser anterior a la fecha de inicio", "OK");
                    return; // Ya no se modifica StartDate, solo se cancela el cambio
                }

                if (_endDateTime != value)
                {
                    _endDateTime = value;
                    OnPropertyChanged(nameof(EndDate));
                    OnPropertyChanged(nameof(EndDatePart));
                    OnPropertyChanged(nameof(EndTimePart));
                }
            }
        }

        /// <summary>
        /// Gets or sets the date part of the end datetime.
        /// </summary>
        public DateTime EndDatePart
        {
            get => _endDateTime.Date;
            set
            {
                EndDate = value.Date + _endDateTime.TimeOfDay;
            }
        }

        /// <summary>
        /// Gets or sets the time part of the end datetime.
        /// </summary>
        public TimeSpan EndTimePart
        {
            get => _endDateTime.TimeOfDay;
            set
            {
                EndDate = _endDateTime.Date + value;
            }
        }

        /// <summary>
        /// Notifies property changes.
        /// </summary>
        /// <param name="propertyName">Property name that changed.</param>
        private void OnPropertyChanged(string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        /// <summary>
        /// Event triggered when a property value changes.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Backing field for error message.
        /// </summary>
        private string _ErrorMessage { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the error message text.
        /// </summary>
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


        private Dictionary<string, string> _Users;

        public Dictionary<string, string> Users
        {
            get { return _Users; }
            set
            {
                if (_Users != value)
                {
                    _Users = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(UsersList)); // <- Notifica el cambio
                }
            }
        }
        public List<KeyValuePair<string, string>> UsersList => _Users?.ToList();

        #endregion


        #region Commands

        /// <summary>
        /// Command to add a new booking.
        /// </summary>
        public Command AddBookingCommand => new Command(async () => await AddBooking());

        /// <summary>
        /// Command to cancel the booking process.
        /// </summary>
        public Command CancelBookingCommand => new Command(async () => await CancelBooking());

        #endregion

        #region Methods

        /// <summary>
        /// Checks if the booking time slot is available via the API.
        /// Prompts the user to confirm adding the booking if available.
        /// </summary>
        private async Task CheckAvariable()
        {
            try
            {
                var urlCar = "api/Cars/available/" + booking.CarId;
                var Car = await aPIService.GetAsync<CarModel>(urlCar, TimeSpan.FromSeconds(24)); /// si es null no esta disponibile

                if (Car == null)
                {
                    await Application.Current.MainPage.DisplayAlert("Error", "Vehículo no disponible", "OK");
                    return;
                }

                var url = $"api/Bookings/Search?startDate={StartDate.ToString("yyyy-MM-ddTHH:mm:ss")}&endDate={EndDate.ToString("yyyy-MM-ddTHH:mm:ss")}";
                var bookings = await aPIService.GetAsync<List<Booking>>(url, TimeSpan.FromSeconds(24));
                if (!bookings.Any())
                {
                    booking.StartDate = StartDate;
                    booking.EndDate = EndDate;
                    booking.UserId = "6f969ce2-53a1-4b39-b8d0-aa0d25c5c4bb";
                    booking.Deleted = false;
                    bool add = await Application.Current.MainPage.DisplayAlert("Advertencia", "¿Desea añadir la reserva?", "Añadir reserva", "Cancelar");
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
        /// <summary>
        /// Initiates adding a booking asynchronously.
        /// </summary>
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

        /// <summary>
        /// Cancels the booking and removes the current page from navigation.
        /// </summary>
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

        /// <summary>
        /// Placeholder for sending booking data asynchronously.
        /// </summary>
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

        private async Task LoadingUsers()
        {
            try
            {
                var api = new APIService();
                var Users = await api.GetAsync<Dictionary<string, string>>("api/GetUsersDic", TimeSpan.FromSeconds(10));

            }
            catch (Exception n)
            {
                await Application.Current.MainPage.DisplayAlert("Error", "Error Interno: " + n.Message, "OK");
            }
        }
        #endregion
    }
}
