

using CheckCars.Models;
using PdfSharpCore.Drawing;
using PdfSharpCore.Pdf;
using PdfSharpCore.Pdf.IO.enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SixLabors.ImageSharp;
using System.Threading.Tasks;
using SixLabors.ImageSharp.Formats.Jpeg;
using Image = SixLabors.ImageSharp.Image;
using SixLabors.ImageSharp.Processing;
using System.Diagnostics;


namespace CheckCars.Utilities
{
    public class PDFGenerate
    {


        public void ResizeImage(string filePath, string outputPath)
        {
            // Cargar la imagen
            using (Image image = Image.Load(filePath))
            {
                // Redimensionar la imagen
                image.Mutate(x => x.Resize(1200, 2400));

                // Guardar la imagen redimensionada
                image.Save(outputPath, new JpegEncoder { Quality = 75 }); // Puedes ajustar la calidad
            }
        }



        public async Task<byte[]> EntryExitReport(EntryExitReport i)
        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

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
                            new Tuple<string, string>("Autor", i.Author ?? ""),
                            new Tuple<string, string>("Fecha  Creación", i.Created.ToString("yyyy-MM-dd")),
                            new Tuple<string, string>("Latitud", i.Latitude.ToString()),
                            new Tuple<string, string>("Longitud", i.Longitude.ToString()),
                            new Tuple<string, string>("Kilometraje", i.mileage.ToString()),
                            new Tuple<string, string>("Vehículo", i.CarPlate ?? ""),
                            new Tuple<string, string>("Nivel Combustible", Math.Round((i.FuelLevel/100)).ToString() + "%"),
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
                            new Tuple<string, string>("Estado de los interiores" +"", i.InteriorsState ?? "")
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
            finally
            {
                var e = $"El tiempo transcurrido es: {stopwatch.ElapsedMilliseconds} ms";
                Application.Current.MainPage.DisplayAlert("Info", e, "Ok");
                stopwatch.Stop();
                Console.WriteLine(e);
            }
        }
     

        private void DrawPage(PdfPage page)
        {
            XGraphics xGraphics = XGraphics.FromPdfPage(page);

        }


    }
}

