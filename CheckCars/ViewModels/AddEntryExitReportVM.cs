using CheckCars.Data;
using CheckCars.Models;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace CheckCars.ViewModels
{
    public class AddEntryExitReportVM : INotifyPropertyChangedAbst
    {

        #region General
        // Implementación de INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        // Método para notificar cambios en las propiedades
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion


        private EntryExitReport _report = new();

        public EntryExitReport Report
        {
            get { return _report; }
            set
            {
                if(_report != value)
                {
                    _report = value;
                    OnPropertyChanged(nameof(Report));
                }
            }
        }

        // Lista para almacenar las fotos capturadas
        private ObservableCollection<Photo> _imgs = new();

        // Propiedad para la lista de fotos
        public ObservableCollection<Photo> ImgList
        {
            get { return _imgs; }
            set
            {
                if (_imgs != value)
                {
                    _imgs = value;
                    OnPropertyChanged(nameof(ImgList));  // Notificamos el cambio de lista
                }
            }
        }
        public ICommand TakePhotoCommand
        {
            get
            {
                return new Command(() => Task.Run(TakePhoto));
            }
            private set { }
        }
        public ICommand AddReport
        {
            get
            {
                return new Command(() => AddReportEntry());
            }
            private set { }
        }

    private async Task AddReportEntry()
        {
            try
            {
                // Muestra un cuadro de diálogo con una pregunta
                bool answer = await Application.Current.MainPage.DisplayAlert(
                    "Confirmación",                   // Título del cuadro de diálogo
                    "¿Deseas continuar?",             // Pregunta
                    "Sí",                             // Texto del botón de confirmación
                    "No"                              // Texto del botón de cancelación
                );
                if (answer)
                {
                    using (var db = new ReportsDBContextSQLite())
                    {
                        //Report.Id = db.EntryExitReports.OrderBy(e=>e.Id).LastOrDefault().Id + 1;
                        db.EntryExitReports.Add(Report);
                        db.SaveChanges();
                        foreach (var item in ImgList)
                        {
                            item.ReportId = Report.ReportId;
                        }
                        db.Photos.AddRange(ImgList);
                        db.SaveChanges();


                        Close();
                    }
                }
            }
            catch (Exception rf) 
            {

            }
        }
    
        private async Task Close()
        {
            var d = Application.Current.MainPage.Navigation.NavigationStack.LastOrDefault();
            Application.Current.MainPage.Navigation.RemovePage(d);
        }

        // Método para capturar y guardar la foto
        private async Task TakePhoto()
        {
            try
            {
                if (MediaPicker.Default.IsCaptureSupported)
                {
                    // Captura la foto
                    FileResult photo = await MediaPicker.Default.CapturePhotoAsync();

                    if (photo != null)
                    {
                        // Obtiene la ruta local del archivo (temporal)
                        string filePath = Path.Combine(FileSystem.AppDataDirectory, photo.FileName);

                        // Guarda la foto en el almacenamiento local
                        using (var stream = await photo.OpenReadAsync())
                        {
                            using (var fileStream = File.OpenWrite(filePath))
                            {
                                await stream.CopyToAsync(fileStream);
                            }
                        }

                        // Crea un objeto Photo con la información de la imagen
                        var newPhoto = new Photo
                        {
                            PhotoId = ImgList.Count + 1, // O cualquier otra lógica para generar PhotoId
                            FileName = photo.FileName,
                            FilePath = filePath,
                            DateTaken = DateTime.Now
                        };

                        // Agrega el objeto Photo a la lista
                        ImgList.Add(newPhoto);
                    }
                }
            }
            catch (Exception ex)
            {
                // Manejo de excepciones, por ejemplo, mostrar un mensaje de error
                Console.WriteLine($"Error al capturar y guardar la foto: {ex.Message}");
            }
        }
    }
}
