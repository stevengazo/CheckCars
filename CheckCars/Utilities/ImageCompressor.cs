using MetadataExtractor;
using MetadataExtractor.Formats.Xmp;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.IO;
using MetadataExtractor;
using MetadataExtractor.Formats.Xmp;
using SkiaSharp;
using MetadataExtractor.Formats.Exif;

namespace CheckCars.Utilities
{
    public class ImageCompressor
    {
        public void CompressJpeg(string imagePath, int compressionPercentage)
        {
            if (compressionPercentage < 0 || compressionPercentage > 100)
            {
                throw new ArgumentOutOfRangeException(nameof(compressionPercentage), "El porcentaje de compresión debe estar entre 0 y 100.");
            }

            // Tamaño original de la imagen
            var originalSize = new FileInfo(imagePath).Length;

            // Leer la imagen original
            using var inputStream = File.OpenRead(imagePath);
            using var originalBitmap = SKBitmap.Decode(inputStream);

            if (originalBitmap == null)
            {
                throw new InvalidOperationException("No se pudo decodificar la imagen original.");
            }


            // Obtener metadatos
            var directories = ImageMetadataReader.ReadMetadata(imagePath);
            var orientation = GetOrientationFromMetadata(directories);

            // Aplicar orientación si es necesario
            using var orientedBitmap = ApplyOrientation(originalBitmap, orientation);

            // Configurar la compresión
            var imageInfo = new SKImageInfo(orientedBitmap.Width, orientedBitmap.Height);
            using var surface = SKSurface.Create(imageInfo);
            using var canvas = surface.Canvas;

            canvas.Clear(SKColors.Transparent);
            canvas.DrawBitmap(orientedBitmap, 0, 0);
            canvas.Flush();

            using var compressedImage = surface.Snapshot();
            using var data = compressedImage.Encode(SKEncodedImageFormat.Jpeg, compressionPercentage);

            File.Delete(imagePath)
    ;

            // Sobrescribir la imagen original
            using var outputStream = File.OpenWrite(imagePath);
            data.SaveTo(outputStream);

            // Tamaño nuevo de la imagen
            var newSize = new FileInfo(imagePath).Length;

            string message = $"Porcentaje de compresión: {compressionPercentage}%\n" +
                                 $"Tamaño original: {originalSize / 1048576.0:F2} MB\n" +
                                 $"Tamaño comprimido: {newSize / 1048576.0:F2} MB\n" +
                                 $"Reducción de tamaño: {((double)(originalSize - newSize) / originalSize * 100):F2}%";

     
        }

        private static int GetOrientationFromMetadata(System.Collections.Generic.IEnumerable<MetadataExtractor.Directory> directories)
        {
            foreach (var directory in directories)
            {
                if (directory is ExifIfd0Directory exifDir)
                {
                    if (exifDir.TryGetInt32(ExifDirectoryBase.TagOrientation, out var orientation))
                    {
                        return orientation;
                    }
                }
            }

            // Si no se encuentra información de orientación, asumir normal
            return 1; // Normal
        }

        private static SKBitmap ApplyOrientation(SKBitmap bitmap, int orientation)
        {
            SKBitmap rotatedBitmap;
            switch (orientation)
            {
                case 3: // Rotar 180 grados
                    rotatedBitmap = new SKBitmap(bitmap.Width, bitmap.Height);
                    using (var canvas = new SKCanvas(rotatedBitmap))
                    {
                        canvas.Translate(bitmap.Width, bitmap.Height);
                        canvas.RotateDegrees(180);
                        canvas.DrawBitmap(bitmap, 0, 0);
                    }
                    return rotatedBitmap;
                case 6: // Rotar 90 grados en sentido horario
                    rotatedBitmap = new SKBitmap(bitmap.Height, bitmap.Width);
                    using (var canvas = new SKCanvas(rotatedBitmap))
                    {
                        canvas.Translate(rotatedBitmap.Width, 0);
                        canvas.RotateDegrees(90);
                        canvas.DrawBitmap(bitmap, 0, 0);
                    }
                    return rotatedBitmap;
                case 8: // Rotar 90 grados en sentido antihorario
                    rotatedBitmap = new SKBitmap(bitmap.Height, bitmap.Width);
                    using (var canvas = new SKCanvas(rotatedBitmap))
                    {
                        canvas.Translate(0, rotatedBitmap.Height);
                        canvas.RotateDegrees(270);
                        canvas.DrawBitmap(bitmap, 0, 0);
                    }
                    return rotatedBitmap;
                default: // No rotación
                    return bitmap;
            }
        }




    }
}
