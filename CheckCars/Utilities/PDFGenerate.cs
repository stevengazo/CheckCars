
using Android.Graphics.Fonts;
using CheckCars.Models;
using PdfSharp.Drawing;
using PdfSharp.Pdf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PdfSharp;

namespace CheckCars.Utilities
{
    public class PDFGenerate
    {
        XFont fuenteTitulo = new XFont("Arial", 12);
        XFont fuentePequena = new XFont("Arial", 12);
        XFont fuenteBold = new XFont("Arial", 12);
        XFont fuenteGeneral = new XFont("Arial", 12);

        public async Task<byte[]> EntryExitReport(EntryExitReport i)
        {
			try
			{
                // Crear un MemoryStream para almacenar el PDF generado
                using (var memoryStream = new MemoryStream())
                {
                    // Crear un documento PDF
                    var document = new PdfDocument();
                    document.PageLayout = PdfPageLayout.SinglePage;
                    document.ViewerPreferences.FitWindow = true;
                    document.Info.Title = "Reporte de Entrada y Salida";

                    // Agregar una página al documento
                    var page = document.AddPage();
                    var graphics = XGraphics.FromPdfPage(page);

                    

                    // Definir posiciones para la tabla de datos
                    double yPos = 20;
                    double xPos = 50;

                    // Agregar título
                    graphics.DrawString("Reporte de Entrada y Salida", new XFont("Arial", 14), XBrushes.Black, new XPoint(xPos, yPos));
                    yPos += 20; // Separar título

                    // Agregar los datos en formato tabla
                    graphics.DrawString("Nombre:", fuenteGeneral, XBrushes.Black, new XPoint(xPos, yPos));
                    graphics.DrawString(i.Name ?? "N/A", fuenteGeneral, XBrushes.Black, new XPoint(xPos + 100, yPos));
                    yPos += 20;

                    graphics.DrawString("Autor:", fuenteGeneral, XBrushes.Black, new XPoint(xPos, yPos));
                    graphics.DrawString(i.Author ?? "N/A", fuenteGeneral, XBrushes.Black, new XPoint(xPos + 100, yPos));
                    yPos += 20;

                    graphics.DrawString("Fecha de Creación:", fuenteGeneral, XBrushes.Black, new XPoint(xPos, yPos));
                    graphics.DrawString(i.Created.ToString("dd/MM/yyyy"), fuenteGeneral, XBrushes.Black, new XPoint(xPos + 100, yPos));
                    yPos += 20;

                    graphics.DrawString("Nivel de Aceite:", fuenteGeneral, XBrushes.Black, new XPoint(xPos, yPos));
                    graphics.DrawString(i.OilLevel ?? "N/A", fuenteGeneral, XBrushes.Black, new XPoint(xPos + 100, yPos));
                    yPos += 20;

                    graphics.DrawString("Notas:", fuenteGeneral, XBrushes.Black, new XPoint(xPos, yPos));
                    graphics.DrawString(i.Notes ?? "N/A", fuenteGeneral, XBrushes.Black, new XPoint(xPos + 100, yPos));
                    yPos += 40; // Más espacio para las notas

                    // Agregar más propiedades de EntryExitReport en el formato de tabla
                    graphics.DrawString("Estado de las Llantas:", fuenteGeneral, XBrushes.Black, new XPoint(xPos, yPos));
                    graphics.DrawString(i.TiresState ?? "N/A", fuenteGeneral, XBrushes.Black, new XPoint(xPos + 100, yPos));
                    yPos += 20;

                    graphics.DrawString("Llantas de Refacción:", fuenteGeneral, XBrushes.Black, new XPoint(xPos, yPos));
                    graphics.DrawString(i.HasSpareTire ? "Sí" : "No", fuenteGeneral, XBrushes.Black, new XPoint(xPos + 100, yPos));
                    yPos += 20;

                    // Agregar otras propiedades de EntryExitReport de manera similar
                    graphics.DrawString("Tiene QuickPass:", fuenteGeneral, XBrushes.Black, new XPoint(xPos, yPos));
                    graphics.DrawString(i.HasQuickPass ? "Sí" : "No", fuenteGeneral, XBrushes.Black, new XPoint(xPos + 100, yPos));
                    yPos += 20;

                 

                    // Guardar el documento PDF en el MemoryStream
                    document.Save(memoryStream, false);

                    // Devolver el PDF como un arreglo de bytes
                    return memoryStream.ToArray();
                }
            }
			catch (Exception r)
			{
                return null;
				
			}
        }
    }
}