using iText.StyledXmlParser.Util;
using Microsoft.EntityFrameworkCore;
using CheckCar.Data;
using CheckCar.Models;
using CheckCar.Services;
using CheckCar.Utilities;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CheckCar.ViewModels
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
            booking = new Booking()
            {
                StartDate = DateTime.Now,
                Status = "Pendiente",
                Deleted = false,
                Confirmed = false,
                EndDate = DateTime.Now.AddHours(1)
            };
            MainThread.BeginInvokeOnMainThread(async () => {

                try
                {
                    carsList = await _db.Cars.Select(e => e.Plate).ToListAsync();
                   await LoadingUsers();
                }
                catch (Exception v)
                {
                    MessageUtilities.ShowLongToast("Error al cargar datos: " + v.Message).Wait();
                }
            });
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public async Task InitializeAsync()
        {
            try
            {
                CarsList = await _db.Cars.Select(e => e.Plate).ToListAsync();
                await LoadingUsers();
            }
            catch (Exception ex)
            {
                await MessageUtilities.ShowLongToast("Error al cargar datos: " + ex.Message);
            }
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
                            await MessageUtilities.ShowToast("Reserva Añadida");
                        }
                    }
                    else
                    {
                        return;
                    }
                }
                else
                {
                    await MessageUtilities.ShowToast("Vehículo no disponible en esas fechas");
                }
            }
            catch (Exception e)
            {
                await MessageUtilities.ShowInfoMessageAsync(MessageUtilities.TitleError, "Error de la aplicación, intente de nuevo." + e.Message);
            }

        }
        /// <summary>
        /// Initiates adding a booking asynchronously.
        /// </summary>
        private async Task AddBooking()
        {
            try
            {
                await CheckAvariable();
            }
            catch (Exception ds)
            {
                await MessageUtilities.ShowInfoMessageAsync(MessageUtilities.TitleError, "Error interno, intente de nuevo. " + ds.Message);
            }
        }

        /// <summary>
        /// Cancels the booking and removes the current page from navigation.
        /// </summary>
        private async Task CancelBooking()
        {
            try
            {
                // Usar la nueva API recomendada para obtener la página principal
                var mainWindow = Application.Current?.Windows.FirstOrDefault();
                var mainPage = mainWindow?.Page;

                if (mainPage?.Navigation?.NavigationStack?.Count > 0)
                {
                    var lastPage = mainPage.Navigation.NavigationStack[mainPage.Navigation.NavigationStack.Count - 1];
                    mainPage.Navigation.RemovePage(lastPage);
                }
            }
            catch (Exception fd)
            {
                await MessageUtilities.ShowInfoMessageAsync(MessageUtilities.TitleError, "Error interno, intente de nuevo. " + fd.Message);
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
                await MessageUtilities.ShowInfoMessageAsync(MessageUtilities.TitleError, "Error interno, intente de nuevo. " + f.Message);
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
                await MessageUtilities.ShowInfoMessageAsync(MessageUtilities.TitleError, "Error interno, intente de nuevo. " + n.Message);
            }
        }
        #endregion
    }
}
