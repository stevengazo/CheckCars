using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace CheckCars.ViewModels
{
    public class INotifyPropertyChangedAbst : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        // Método protegido, de modo que solo las clases derivadas o la propia clase puedan invocar este evento.
        protected void OnPropertyChanged([CallerMemberName] string name = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
