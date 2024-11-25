using vehiculosmecsa.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Maui.Storage;
using Microsoft.Maui.Media;
using Microsoft.Maui.Devices.Sensors;

namespace vehiculosmecsa.Utilities
{
   public class SensorManager
    {

        public CancellationTokenSource _cancelTokenSource;
        public bool _isCheckingLocation;

        public async Task<double[]> GetCurrentLocation()
        {
            try
            {
                _isCheckingLocation = true;
                GeolocationRequest request = new GeolocationRequest(GeolocationAccuracy.Medium, TimeSpan.FromSeconds(10));
                _cancelTokenSource = new CancellationTokenSource();
                Location location = await Geolocation.Default.GetLocationAsync(request, _cancelTokenSource.Token);
                return new double[] { location.Latitude, location.Longitude };
            }
            // Catch one of the following exceptions:
            //   FeatureNotSupportedException
            //   FeatureNotEnabledException
            //   PermissionException
            catch (Exception ex)
            {
                // Unable to get location
                return null;
            }
            finally
            {
                _isCheckingLocation = false;
            }
        }
        public void CancelRequest()
        {
            if (_isCheckingLocation && _cancelTokenSource != null && _cancelTokenSource.IsCancellationRequested == false)
                _cancelTokenSource.Cancel();
        }
        private async Task CheckPermissionsAsync()
        {
            // Verifica si el permiso de la cámara ha sido concedido
            var status = await Permissions.RequestAsync<Permissions.Camera>();

            // Si el permiso no ha sido concedido
            if (status != PermissionStatus.Granted)
            {
                // Aquí puedes manejar el caso cuando no se haya concedido el permiso
                // Por ejemplo, mostrar un mensaje al usuario indicando que el permiso es necesario
                await Application.Current.MainPage.DisplayAlert("Permiso requerido", "Se necesita permiso para acceder a la cámara.", "OK");
            }
            else
            {
                // El permiso ha sido concedido
                // Aquí puedes proceder con la lógica para acceder a la cámara
            }
        }


        public async Task<Photo> TakePhoto()
        {
            try
            {
               await CheckPermissionsAsync();
                // Verifica si la captura de fotos está soportada
                if (MediaPicker.Default.IsCaptureSupported)
                {
                    // Captura la foto
                    FileResult photo = await MediaPicker.Default.CapturePhotoAsync();
                    photo.FileName = $"Img-{DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss")}.jpeg";

                    if (photo != null)
                    {
           
                        Directory.CreateDirectory(FileSystem.AppDataDirectory + "/Photos");
                        string filePath = Path.Combine(FileSystem.AppDataDirectory, "Photos", photo.FileName);

                        // Guarda la foto en la ruta local
                        using (var stream = await photo.OpenReadAsync())
                        using (var fileStream = File.OpenWrite(filePath))
                        {
                            await stream.CopyToAsync(fileStream);
                        }
                       
                        // Crea un objeto Photo con la información de la imagen
                        var newPhoto = new Photo
                        {
                            FileName = photo.FileName,
                            FilePath = filePath,
                            DateTaken = DateTime.Now
                        };

                        return newPhoto;
                    }

                    // Si no se captura ninguna foto, devuelve null
                    return null;
                }

                // Si la captura no está soportada, devuelve null
                return null;
            }
            catch (Exception ex)
            {
                // Manejo de excepciones, se puede mostrar un mensaje de error o loguear el error
                Console.WriteLine($"Error al capturar y guardar la foto: {ex.Message}");
                return null;
            }
        }
    }
}
