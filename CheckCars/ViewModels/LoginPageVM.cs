﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace CheckCars.ViewModels
{
   public class LoginPageVM : INotifyPropertyChangedAbst
    {
        public ICommand Login { get; } = new Command(async () =>
        {
            Application.Current.MainPage = new AppShell();
        });
    }
}
