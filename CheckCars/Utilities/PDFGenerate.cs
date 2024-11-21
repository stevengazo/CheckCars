

using CheckCars.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using iText.Kernel.Pdf;
using iText.Commons.Actions.Processors;
using iText.Layout;
using iText.Layout.Element;
using iText.IO.Image;
using Cell = iText.Layout.Element.Cell;
using iText.Layout.Properties;
using System.Reflection;
using iText.Kernel.Colors;


namespace CheckCars.Utilities
{
    public class PDFGenerate
    {


        public void ResizeImage(string filePath, string outputPath)
        {

        }


        private void GenerateTableEntry()
        {
           

        }

        private void AddImages()
        {
               
        }

        public async Task<byte[]> EntryExitReport(EntryExitReport i)
        {
            try
            {
                using (MemoryStream ms = new MemoryStream())
                {
                    PdfWriter writer = new PdfWriter(ms);
                    PdfDocument pdf = new PdfDocument(writer);
                    Document document = new Document(pdf);


                    Paragraph header = new Paragraph("Reporte Vehicular")
                        .SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER)
                        .SetFontSize(20);
                    document.Add(header);

                    // Crear una tabla con 3 columnas
                    Table table = new Table(UnitValue.CreatePercentArray(2)).UseAllAvailableWidth();

                    // Agregar encabezados
                    table.AddHeaderCell("Categorias");
                    table.AddHeaderCell("Datos");


                    table.AddCell(nameof(i.ReportId)).SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER).SetBackgroundColor(new DeviceRgb(173, 216, 230));
                    table.AddCell(i.ReportId);
                    table.AddCell(nameof(i.Longitude)).SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER);
                    table.AddCell(i.Longitude.ToString()).SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER);
                    table.AddCell(nameof(i.Latitude));
                    table.AddCell(i.Latitude.ToString()).SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER);
                    table.AddCell(nameof(i.Created));
                    table.AddCell(i.Created.ToString()).SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER);
                    table.AddCell(nameof(i.CarPlate));
                    table.AddCell(i.CarPlate);


                    document.Add(table);


                    foreach (var ITEM in i.Photos)
                    {
                        if(File.Exists(ITEM.FilePath))
                        {
                            document.Add(new AreaBreak(iText.Layout.Properties.AreaBreakType.NEXT_PAGE));
                            var imgStream = File.ReadAllBytes(ITEM.FilePath);
                            iText.Layout.Element.Image image = new iText.Layout.Element.Image(ImageDataFactory
                                .Create(imgStream))
                                .SetRotationAngle(0)
                                .SetAutoScale(true)
                                .SetHorizontalAlignment(iText.Layout.Properties.HorizontalAlignment.CENTER);

                            document.Add(image);
                        }
                       
                    }

                    document.Close();
                    return ms.ToArray();



                }
            }
            catch (Exception e)
            {

                throw;
            }
        }

      
    }
}

