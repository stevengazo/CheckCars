

using CheckCars.Models;
using PdfSharpCore.Drawing;
using PdfSharpCore.Pdf;
using PdfSharpCore.Pdf.IO.enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;
using Image = SixLabors.ImageSharp.Image;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Processing.Processors.Transforms;

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
                image.Mutate(x => x.Resize(image.Width/2,image.Height/2));
                // Guardar la imagen redimensionada
                image.Save(outputPath, new JpegEncoder { Quality = 80 }); // Puedes ajustar la calidad
            }
        }


        private void GenerateTableEntry(PdfSharpCore.Pdf.PdfPage page , EntryExitReport  Report)
        {
            XGraphics xGrap = XGraphics.FromPdfPage(page);

            // Definir un margen
            double margin = 40;
            double yPosition = margin;

            // Calcular el ancho de la página
            double pageWidth = page.Width;

            // Calcular el ancho de la tabla
            double tableWidth = 150 + 200; // 150 para las propiedades y 200 para los valores
            double tableX = (pageWidth - tableWidth) / 2; // Centrado de la tabla en la página

            // Dibujar el título del reporte
            xGrap.DrawString("Reporte de Entrada y Salida", new XFont("OpenSans", 16, XFontStyle.Bold), XBrushes.Black, new XPoint(margin, yPosition));
            yPosition += 30;

            // Crear una lista de propiedades y sus valores
            var rows = new List<Tuple<string, string>>
                        {
                            new Tuple<string, string>("Id de Reporte", Report.ReportId.ToString()),
                            new Tuple<string, string>("Autor", Report.Author ?? ""),
                            new Tuple<string, string>("Fecha  Creación", Report.Created.ToString("yyyy-MM-dd")),
                            new Tuple<string, string>("Latitud", Report.Latitude.ToString()),
                            new Tuple<string, string>("Longitud", Report.Longitude.ToString()),
                            new Tuple<string, string>("Kilometraje", Report.mileage.ToString()),
                            new Tuple<string, string>("Vehículo", Report.CarPlate ?? ""),
                            new Tuple<string, string>("Nivel Combustible", (Report.FuelLevel/100).ToString() + "%"),
                            new Tuple<string, string>("Notas", Report.Notes ?? ""),
                            new Tuple<string, string>("Tiene Cargador USB", Report.HasChargerUSB.ToString()),
                            new Tuple<string, string>("Tiene QuickPass", Report.HasQuickPass.ToString()),
                            new Tuple<string, string>("Tiene Soporte Teléfono", Report.HasPhoneSupport.ToString()),
                            new Tuple<string, string>("Estado de las llantas", Report.TiresState ?? ""),
                            new Tuple<string, string>("Tiene Llanta Repuesto", Report.HasSpareTire.ToString()),
                            new Tuple<string, string>("Tiene Kit de Emergencias", Report.HasEmergencyKit.ToString()),
                            new Tuple<string, string>("Estado de las pinturas", Report.PaintState ?? ""),
                            new Tuple<string, string>("Estado Mecánico", Report.MecanicState ?? ""),
                            new Tuple<string, string>("Nivel Aceite", Report.OilLevel ?? ""),
                            new Tuple<string, string>("Estado de los interiores" +"", Report.InteriorsState ?? "")
                        };

            // Dibujar cada fila de la tabla (propiedad - valor) con bordes
            foreach (var row in rows)
            {
                double cellHeight = 20; // Altura de cada celda

                // Dibujar el borde de la celda para la propiedad
                xGrap.DrawRectangle(XPens.Black, tableX, yPosition, 150, cellHeight); // Borde de la propiedad
                xGrap.DrawString(row.Item1, new XFont("OpenSans", 10, XFontStyle.Bold), XBrushes.Black,
                    new XPoint(tableX + 5, yPosition + cellHeight / 2 + 3)); // Ajuste de alineación vertical

                // Dibujar el borde de la celda para el valor
                xGrap.DrawRectangle(XPens.Black, tableX + 150, yPosition, 200, cellHeight); // Borde del valor
                xGrap.DrawString(row.Item2, new XFont("OpenSans", 10), XBrushes.Black,
                    new XPoint(tableX + 155, yPosition + cellHeight / 2 + 3)); // Ajuste de alineación vertical

                yPosition += cellHeight; // Saltar una línea después de cada propiedad
            }

        }

        private void AddImages(PdfSharpCore.Pdf.PdfDocument pdfDocument, List<Photo> photos)
        {
            try
            {
                foreach (var photo in photos)
                {
                    if (File.Exists(photo.FilePath))
                    {
                        PdfPage page = pdfDocument.AddPage();
                        XGraphics xGr = XGraphics.FromPdfPage(page);
                        var tmpPath = Path.Combine(FileSystem.CacheDirectory, photo.FileName);
                       // ResizeImage(photo.FilePath, tmpPath);
                        if(File.Exists(photo.FilePath))
                        {
                            var image = XImage.FromFile(photo.FilePath);
                            xGr.DrawImage(image, 10, 10, image.PixelWidth, image.PixelHeight);
                        }
                    }
                }
            }
            catch (Exception e)
            {

                throw;
            }     
        }

        public async Task<byte[]> EntryExitReport(EntryExitReport i)
        {
            try
            {
                using (PdfDocument document = new PdfDocument())
                {
                    PdfPage page = document.AddPage();
                    GenerateTableEntry(page, i);
                    if(i.Photos?.Count> 0)
                    {
                        AddImages(document, i.Photos.ToList());
                    }
                   // Create a MemoryStream for save the byte array
                    using (MemoryStream stream = new MemoryStream())
                    {
                        document.Save(stream);
                        return stream.ToArray();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al generar el reporte: {ex.Message}");
                return null;
            }
        }
    }
}

