using CheckCars.Models;

namespace CheckCars.Utilities
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
            catch (FeatureNotEnabledException r)
            {
                Console.WriteLine(r.Message);
                await Application.Current.MainPage.DisplayAlert("Advertencia", "No fue posible obtener la ubicación", "OK");
                return null;
            }
            catch (FeatureNotSupportedException r)
            {
                await Application.Current.MainPage.DisplayAlert("Advertencia", "No es posible obtener la ubicación" + r.InnerException, "OK");
                return null;
            }
            catch (PermissionException e)
            {
                await Application.Current.MainPage.DisplayAlert("Advertencia", "No posee permisos de la cámara. " + e.InnerException, "OK");
                return null;
            }
            catch (Exception)
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




        public async Task<Photo> TakePhoto()
        {
            try
            {
                // Verifica si la captura de fotos está soportada
                if (MediaPicker.Default.IsCaptureSupported)
                {
                    try
                    {
                        // Captura la foto
                        FileResult photo = await MediaPicker.Default.CapturePhotoAsync();
                        photo.FileName = $"Img-{DateTime.Now:yyyy-MM-dd-HH-mm-ss}.jpeg";

                        if (photo != null)
                        {
                            Directory.CreateDirectory(Path.Combine(FileSystem.AppDataDirectory, "Photos"));
                            string filePath = Path.Combine(FileSystem.AppDataDirectory, "Photos", photo.FileName);

                            // Guarda la foto en la ruta local
                            using (var stream = await photo.OpenReadAsync())
                            using (var fileStream = File.OpenWrite(filePath))
                            {
                                await stream.CopyToAsync(fileStream);
                            }

                            // Crea un objeto Photo con la información de la imagen
                            return new Photo
                            {
                                FileName = photo.FileName,
                                FilePath = filePath,
                                DateTaken = DateTime.Now
                            };
                        }

                        // Si no se captura ninguna foto, devuelve null
                        return null;
                    }
                    catch (OperationCanceledException)
                    {
                        Console.WriteLine("La captura de la foto fue cancelada por el usuario.");
                        return null;
                    }
                    catch (IOException ex)
                    {
                        Console.WriteLine($"Error de almacenamiento: {ex.Message}");
                        throw new Exception("Ocurrió un problema al guardar la foto. Por favor, libere espacio en su dispositivo.");
                    }
                    catch (UnauthorizedAccessException ex)
                    {
                        Console.WriteLine($"Permiso denegado: {ex.Message}");
                        throw new Exception("No se pudo acceder a la cámara o almacenamiento. Por favor, revise los permisos.");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error inesperado durante la captura: {ex.Message}");
                        throw new Exception("Ocurrió un error inesperado al capturar la foto.");
                    }
                }
                else
                {
                    Console.WriteLine("La captura de fotos no está soportada en este dispositivo.");
                    throw new NotSupportedException("Captura de fotos no soportada en este dispositivo.");
                }
            }
            catch (NotSupportedException ex)
            {
                Console.WriteLine($"Error de soporte: {ex.Message}");
                throw;
            }
            catch (UnauthorizedAccessException ex)
            {
                Console.WriteLine($"Error de permisos: {ex.Message}");
                throw;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error general: {ex.Message}");
                throw;
            }
        }

    }
}
