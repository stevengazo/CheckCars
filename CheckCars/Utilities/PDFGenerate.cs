

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


        private void addEntryTable(Document document, EntryExitReport i)
        {
            // Crear una tabla con 3 columnas
            Table table = new Table(UnitValue.CreatePercentArray(2)).UseAllAvailableWidth();

            // Agregar encabezados
            table.AddHeaderCell("Categorias");
            table.AddHeaderCell("Datos");

            AddRow(table, nameof(i.ReportId), i.ReportId);
            AddRow(table, nameof(i.Author), i.Author);
            AddRow(table, nameof(i.Created), i.Created.ToString("yyyy-MMM-dd HH:mm:ss"));
            AddRow(table, nameof(i.CarPlate), i.CarPlate);
            AddRow(table, nameof(i.Latitude), i.Latitude.ToString());
            AddRow(table, nameof(i.Longitude), i.Longitude.ToString());
            AddRow(table, "Kilometraje", i.mileage.ToString());
            AddRow(table, nameof(i.FuelLevel), Math.Round(i.FuelLevel * 1) + "%");
            AddRow(table, nameof(i.HasChargerUSB), i.HasChargerUSB ? "Tiene Cargador USB" : "No Tiene Cargador USB");
            AddRow(table, nameof(i.HasQuickPass), i.HasQuickPass ? "Tiene Quick Pass" : "No tiene quickpass");
            AddRow(table, nameof(i.TiresState), i.TiresState);
            AddRow(table, nameof(i.HasSpareTire), i.HasSpareTire ? "Tiene llanta repuesto" : "No tiene llanta");
            AddRow(table, nameof(i.HasEmergencyKit), i.HasEmergencyKit ? "Tiene kit de emergencia" : "No tiene kit");
            AddRow(table, nameof(i.PaintState), i.PaintState);
            AddRow(table, nameof(i.MecanicState), i.MecanicState);
            AddRow(table, nameof(i.OilLevel), i.OilLevel);
            AddRow(table, nameof(i.InteriorsState), i.InteriorsState);
            document.Add(table);
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

                    Paragraph header = new Paragraph($"Reporte Salida - {i.CarPlate} ") 
                        .SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER)
                        .SetFontSize(20);
                    document.Add(header);

                    addEntryTable(document, i);
                   
                    if(i.Photos.Count> 0)
                    {
                        AddPhotos(document, i.Photos.ToList(),i.CarPlate, i.Created);

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

        private void AddPhotos( Document document , List<Photo> Photos, string CarPlate, DateTime dateCreated)
        {
            foreach (var ITEM in Photos)
            {
                if (File.Exists(ITEM.FilePath))
                {
                    document.Add(new AreaBreak(iText.Layout.Properties.AreaBreakType.NEXT_PAGE));
                    Paragraph head = new Paragraph($"Reporte Vehicular - {CarPlate} ").SetHorizontalAlignment(iText.Layout.Properties.HorizontalAlignment.CENTER);
                    document.Add(head);
                    var imgStream = File.ReadAllBytes(ITEM.FilePath);
                    iText.Layout.Element.Image image = new iText.Layout.Element.Image(ImageDataFactory
                        .Create(imgStream))
                        .SetPadding(5)
                        .SetMargins(10, 10, 10, 10)
                        .SetRotationAngle((270) * 3.1416 / 180)
                        .SetAutoScale(true)
                        .SetHorizontalAlignment(iText.Layout.Properties.HorizontalAlignment.CENTER);
                    document.Add(image);
                    Paragraph dateinfo = new Paragraph(dateCreated.ToString("yyyy-MMM-dd HH:mm:ss")).SetHorizontalAlignment(iText.Layout.Properties.HorizontalAlignment.CENTER).SetFontSize(8);
                    document.Add(dateinfo);

                }
            }

        }


          private void AddRow(Table table, string Title, string value)
        {
            table.AddCell(Title).SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER);
            table.AddCell(value);
        }
      
    }
}

