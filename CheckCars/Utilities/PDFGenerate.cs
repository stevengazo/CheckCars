

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
                new Tuple<string, string>("ReportId", i.ReportId.ToString()),
                new Tuple<string, string>("Name", i.Name ?? ""),
                new Tuple<string, string>("Author", i.Author ?? ""),
                new Tuple<string, string>("Created", i.Created.ToString("yyyy-MM-dd")),
                new Tuple<string, string>("Mileage", i.mileage.ToString()),
                new Tuple<string, string>("CarPlate", i.CarPlate ?? ""),
                new Tuple<string, string>("FuelLevel", i.FuelLevel.ToString()),
                new Tuple<string, string>("Notes", i.Notes ?? ""),
                new Tuple<string, string>("HasChargerUSB", i.HasChargerUSB.ToString()),
                new Tuple<string, string>("HasQuickPass", i.HasQuickPass.ToString()),
                new Tuple<string, string>("HasPhoneSupport", i.HasPhoneSupport.ToString()),
                new Tuple<string, string>("TiresState", i.TiresState ?? ""),
                new Tuple<string, string>("HasSpareTire", i.HasSpareTire.ToString()),
                new Tuple<string, string>("HasEmergencyKit", i.HasEmergencyKit.ToString()),
                new Tuple<string, string>("PaintState", i.PaintState ?? ""),
                new Tuple<string, string>("MecanicState", i.MecanicState ?? ""),
                new Tuple<string, string>("OilLevel", i.OilLevel ?? ""),
                new Tuple<string, string>("InteriorsState", i.InteriorsState ?? "")
            };

                    // Dibujar cada fila de la tabla (propiedad - valor) con bordes
                    foreach (var row in rows)
                    {
                        // Dibujar el borde de la celda para la propiedad
                        gfx.DrawRectangle(XPens.Black, tableX, yPosition, 150, 20); // Borde de la propiedad
                        gfx.DrawString(row.Item1, new XFont("OpenSans", 10, XFontStyle.Bold), XBrushes.Black, new XPoint(tableX + 5, yPosition + 5));

                        // Dibujar el borde de la celda para el valor
                        gfx.DrawRectangle(XPens.Black, tableX + 150, yPosition, 200, 20); // Borde del valor
                        gfx.DrawString(row.Item2, new XFont("OpenSans", 10), XBrushes.Black, new XPoint(tableX + 155, yPosition + 5));

                        yPosition += 20;  // Saltar una línea después de cada propiedad
                    }

                    // Si tiene fotos, agregar las imágenes al documento
                    if (i.Photos != null && i.Photos.Any())
                    {
                        yPosition += 20; // Separar un poco para las fotos
                        gfx.DrawString("Fotos:", new XFont("OpenSans", 12, XFontStyle.Bold), XBrushes.Black, new XPoint(margin, yPosition));
                        yPosition += 20;

                        foreach (var photo in i.Photos)
                        {
                            // Asegurarse de que la ruta de la foto sea válida
                            if (File.Exists(photo.FilePath))
                            {
                                XImage image = XImage.FromFile(photo.FilePath);
                                gfx.DrawImage(image, margin, yPosition);  // Tamaño de la imagen ajustable
                                yPosition += 110;  // Ajustar el salto de línea después de la imagen
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

