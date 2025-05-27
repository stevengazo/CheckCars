using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace CheckCars.ViewModels
{
    /// <summary>
    /// Base class implementing INotifyPropertyChanged to simplify property change notification in ViewModels.
    /// </summary>
    public class INotifyPropertyChangedAbst : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Raises the PropertyChanged event to notify the UI that a property value has changed.
        /// </summary>
        /// <param name="propertyName">Name of the property that changed. Automatically provided by CallerMemberName.</param>
        protected void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
