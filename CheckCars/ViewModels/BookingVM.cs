using Plugin.Maui.Calendar.Models;
using CheckCars.Models;
using CheckCars.Views;
using System.Windows.Input;
using CheckCars.Services;
using CheckCars.Data;
using Microsoft.EntityFrameworkCore;

namespace CheckCars.ViewModels
{
    /// <summary>
    /// ViewModel responsible for managing booking data and events.
    /// </summary>
    public class BookingVM : INotifyPropertyChangedAbst
    {

        #region Properties

        /// <summary>
        /// Service for API communication.
        /// </summary>
        private readonly APIService aPIService = new APIService();

        /// <summary>
        /// Database context for local storage.
        /// </summary>
        private readonly ReportsDBContextSQLite _db = new();

        private EventCollection _events = new EventCollection();

        /// <summary>
        /// Collection of events grouped by date.
        /// </summary>
        public EventCollection Events
        {
            get => _events;
            set
            {
                if (_events != value)
                {
                    _events = value;
                    OnPropertyChanged(nameof(Events));
                }
            }
        }

        private DateTime _SelectedDate;

        /// <summary>
        /// Selected date property, notifies on change.
        /// </summary>
        public DateTime SelectedDate
        {
            get { return _SelectedDate; }
            set
            {
                if (_SelectedDate != value)
                {
                    _SelectedDate = value;
                    OnPropertyChanged(nameof(SelectedDate));
                }
            }
        }
        #endregion

        #region Commands

        /// <summary>
        /// Command to add a new booking, navigates to AddBooking page.
        /// </summary>
        public ICommand AddBooking { get; } = new Command(async () => await Application.Current.MainPage.Navigation.PushAsync(new AddBooking(), true));

        /// <summary>
        /// Command to update bookings by refreshing data.
        /// </summary>
        public ICommand UpdateReports => new Command(async () => await UpdateBookings());

        #endregion

        #region Methods

        /// <summary>
        /// Updates bookings asynchronously. (Not implemented)
        /// </summary>
        private async Task UpdateBookings()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Updates bookings filtered by a specific date. (Not implemented)
        /// </summary>
        /// <param name="d">Date to filter bookings by.</param>
        public async Task UpdateBookings(DateTime d)
        {
            // Update the list of TmpBookings by the month and year
            throw new NotImplementedException();
        }

        /// <summary>
        /// Saves a list of bookings to the local database asynchronously.
        /// </summary>
        /// <param name="bookings">List of bookings to save.</param>
        private async Task SaveInDBAsync(List<Booking> bookings)
        {
            try
            {
                _db.Bookings.RemoveRange(_db.Bookings.ToList());
                if (bookings != null)
                {
                    var ExistingIds = _db.Bookings.Select(b => b.BookingId).ToHashSet();
                    var newBookings = bookings.Where(b => !ExistingIds.Contains(b.BookingId)).ToList();

                    if (newBookings.Count > 0)
                    {
                        _db.Bookings.AddRange(newBookings);
                    }

                    await _db.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("Error", ex.Message, "OK");
            }
        }

        /// <summary>
        /// Loads bookings data into the Events collection grouping bookings by date.
        /// </summary>
        /// <param name="bookings">List of bookings to load.</param>
        private async Task LoadDataAsync(List<Booking> bookings)
        {
            if (bookings != null)
            {
                var eventos = new EventCollection();

                foreach (var booking in bookings)
                {
                    // Iterate through all days from Startdate to EndDate inclusive
                    for (DateTime date = booking.Startdate.Date; date <= booking.EndDate.Date; date = date.AddDays(1))
                    {
                        if (!eventos.ContainsKey(date))
                            eventos[date] = new List<Booking>(); // Changed ICollection to List<Booking>

                        ((List<Booking>)eventos[date]).Add(booking); // Explicit cast to List<Booking>
                    }
                }

                Events = eventos;
                await Application.Current.MainPage.DisplayAlert("Info", "Hay reservas", "OK");
            }
            else
            {
                await Application.Current.MainPage.DisplayAlert("Error", "No hay reservas", "OK");
            }
        }

        /// <summary>
        /// Retrieves booking data from API and updates local database and events collection.
        /// </summary>
        private async Task GetData()
        {
            var api = new APIService();
            var Reservas = await api.GetAsync<List<Booking>>("api/Bookings", TimeSpan.FromSeconds(55));
            await SaveInDBAsync(Reservas);
            List<Booking> TmpBookings = await _db.Bookings.Include(b => b.Car).ToListAsync();
            await LoadDataAsync(TmpBookings);
        }

        #endregion

        #region Constructor

        /// <summary>
        /// Constructor initializes the ViewModel and loads booking data asynchronously.
        /// </summary>
        public BookingVM()
        {
            try
            {
                Task.Run(async () => await GetData());
            }
            catch (Exception gt)
            {
                Application.Current.MainPage.DisplayAlert("Error", gt.Message, "OK");
                throw;
            }
        }

        #endregion

    }

}
