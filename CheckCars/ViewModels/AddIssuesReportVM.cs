using CheckCars.Data;
using CheckCars.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace CheckCars.ViewModels
{
    public class AddIssuesReportVM : INotifyPropertyChangedAbst
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

        public AddIssuesReportVM()
        {
            
        }

        private IssueReport _newIssueReport = new();
        public IssueReport newIssueReport
        {
            get { return _newIssueReport; }
            set
            {
                _newIssueReport = value;
                if (_newIssueReport != value)
                {
                    OnPropertyChanged(nameof(newIssueReport));
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
                bool answer = await Application.Current.MainPage.DisplayAlert(
                    "Confirmación",
                    "¿Deseas continuar?",
                    "Sí",
                    "No"
                );

                if (answer)
                {
                    newIssueReport.Name = "Registro de Entrada y Salida";
                    newIssueReport.Author = "Temporal";
                    using (var db = new ReportsDBContextSQLite())
                    {
                        // Asegura que ImgList tenga PhotoId autogenerado en la base de datos
                        newIssueReport.Photos = ImgList.Select(photo =>
                        {
                            photo.PhotoId = 0;  // Reset para que se genere automáticamente
                            return photo;
                        }).ToList();

                        db.IssueReports.Add(newIssueReport);
                        db.SaveChanges();
                        Close();
                    }
                }
            }
            catch (Exception rf)
            {
                Application.Current.MainPage.DisplayAlert("Error", rf.Message, "ok");
            }
        }

        private async Task Close()
        {
            var d = Application.Current.MainPage.Navigation.NavigationStack.LastOrDefault();
            Application.Current.MainPage.Navigation.RemovePage(d);
        }
    }
}
