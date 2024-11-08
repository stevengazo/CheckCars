

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

                    // Usar una fuente genérica que no dependa del sistema de fuentes

                  
                    XFont font = new XFont("OpenSans", 12); // Usar Arial o alguna otra fuente común

                    // Escribir texto en la página
                    gfx.DrawString("¡Hola, Mundo!", font, XBrushes.Black, new XPoint(100, 100));

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

            catch (Exception r)
			{
                return null;
				
			}
        }
    }
}

