

using CheckCars.Models;
using PdfSharpCore.Drawing;
using PdfSharpCore.Pdf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace CheckCars.Utilities
{
    public class PDFGenerate
    {

        public async Task<byte[]> EntryExitReport(EntryExitReport i)
        {
            try
            {
                // Crear un documento PDF
                using (PdfDocument document = new PdfDocument())
                {
                    // Agregar una página
                    PdfPage page = document.AddPage();

                    // Obtener el objeto de dibujo para la página
                    XGraphics gfx = XGraphics.FromPdfPage(page);

                    // Establecer la fuente (asegurarse de usar la fuente personalizada cargada)
                    XFont font = new XFont("OpenSans", 12);

                    // Definir un margen
                    double margin = 40;
                    double yPosition = margin;

                    // Calcular el ancho de la página
                    double pageWidth = page.Width;

                    // Calcular el ancho de la tabla
                    double tableWidth = 150 + 200; // 150 para las propiedades y 200 para los valores
                    double tableX = (pageWidth - tableWidth) / 2; // Centrado de la tabla en la página

                    // Dibujar el título del reporte
                    gfx.DrawString("Reporte de Entrada y Salida", new XFont("OpenSans", 16, XFontStyle.Bold), XBrushes.Black, new XPoint(margin, yPosition));
                    yPosition += 30;

                    // Crear una lista de propiedades y sus valores
                    var rows = new List<Tuple<string, string>>
                        {
                            new Tuple<string, string>("Id de Reporte", i.ReportId.ToString()),
                            new Tuple<string, string>("Nombre", i.Name ?? ""),
                            new Tuple<string, string>("Autor", i.Author ?? ""),
                            new Tuple<string, string>("Fecha  Creación", i.Created.ToString("yyyy-MM-dd")),
                            new Tuple<string, string>("Kilometraje", i.mileage.ToString()),
                            new Tuple<string, string>("Vehículo", i.CarPlate ?? ""),
                            new Tuple<string, string>("Nivel Combustible", (i.FuelLevel/100).ToString() + "%"),
                            new Tuple<string, string>("Notas", i.Notes ?? ""),
                            new Tuple<string, string>("Tiene Cargador USB", i.HasChargerUSB.ToString()),
                            new Tuple<string, string>("Tiene QuickPass", i.HasQuickPass.ToString()),
                            new Tuple<string, string>("Tiene Soporte Teléfono", i.HasPhoneSupport.ToString()),
                            new Tuple<string, string>("Estado de las llantas", i.TiresState ?? ""),
                            new Tuple<string, string>("Tiene Llanta Repuesto", i.HasSpareTire.ToString()),
                            new Tuple<string, string>("Tiene Kit de Emergencias", i.HasEmergencyKit.ToString()),
                            new Tuple<string, string>("Estado de las pinturas", i.PaintState ?? ""),
                            new Tuple<string, string>("Estado Mecánico", i.MecanicState ?? ""),
                            new Tuple<string, string>("Nivel Aceite", i.OilLevel ?? ""),
                            new Tuple<string, string>("Estado de los interiores" +
                            "", i.InteriorsState ?? "")
                        };

                    // Dibujar cada fila de la tabla (propiedad - valor) con bordes
                    foreach (var row in rows)
                    {
                        double cellHeight = 20; // Altura de cada celda

                        // Dibujar el borde de la celda para la propiedad
                        gfx.DrawRectangle(XPens.Black, tableX, yPosition, 150, cellHeight); // Borde de la propiedad
                        gfx.DrawString(row.Item1, new XFont("OpenSans", 10, XFontStyle.Bold), XBrushes.Black,
                            new XPoint(tableX + 5, yPosition + cellHeight / 2 + 3)); // Ajuste de alineación vertical

                        // Dibujar el borde de la celda para el valor
                        gfx.DrawRectangle(XPens.Black, tableX + 150, yPosition, 200, cellHeight); // Borde del valor
                        gfx.DrawString(row.Item2, new XFont("OpenSans", 10), XBrushes.Black,
                            new XPoint(tableX + 155, yPosition + cellHeight / 2 + 3)); // Ajuste de alineación vertical

                        yPosition += cellHeight; // Saltar una línea después de cada propiedad
                    }



                    if (i.Photos != null && i.Photos.Any())
                    {
                        foreach (var photo in i.Photos)
                        {
                            // Verifica que la ruta de la foto sea válida
                            if (File.Exists(photo.FilePath))
                            {
                                // Crea una nueva página en el documento
                                PdfPage page1 = document.AddPage();
                                XGraphics gfx1 = XGraphics.FromPdfPage(page1);

                                // Carga la imagen
                                XImage image = XImage.FromFile(photo.FilePath);

                                // Dimensiones de la página
                                double pageWidth1 = page1.Width.Point;
                                double pageHeight1 = page1.Height.Point;

                                // Dimensiones originales de la imagen
                                double originalImageWidth = image.PixelWidth;
                                double originalImageHeight = image.PixelHeight;

                                // Dimensiones máximas para la imagen en la página
                                double imageWidth1 = 300;  // Cambia este valor según el ancho máximo deseado
                                double imageHeight1 = 600;  // Cambia este valor según el alto máximo deseado

                                // Calcula el factor de escala para conservar la relación de aspecto
                                double scaleFactor = Math.Min(imageWidth1 / originalImageWidth, imageHeight1 / originalImageHeight);
                                double adjustedImageWidth = originalImageWidth * scaleFactor;
                                double adjustedImageHeight = originalImageHeight * scaleFactor;

                                // Calcula la posición para centrar la imagen en la página
                                double xPosition1 = (pageWidth1 - adjustedImageWidth) / 2;
                                double yPosition1 = (pageHeight1 - adjustedImageHeight) / 2;

                                // Dibuja la imagen en la página, centrada y con el tamaño ajustado
                                gfx1.DrawImage(image, xPosition1, yPosition1, adjustedImageWidth, adjustedImageHeight);

                                // Libera la imagen después de usarla
                                image.Dispose();
                            }
                        }
                    }


                    // Crear un MemoryStream para almacenar el byte array
                    using (MemoryStream stream = new MemoryStream())
                    {
                        // Guardar el documento PDF en el MemoryStream
                        document.Save(stream);

                        // Devolver el PDF como un byte array
                        return stream.ToArray();
                    }
                }
            }
            catch (Exception ex)
            {
                // Manejo de errores
                Console.WriteLine($"Error al generar el reporte: {ex.Message}");
                return null;
            }
        }




    }
}

