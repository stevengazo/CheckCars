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
