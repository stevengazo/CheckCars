using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

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
