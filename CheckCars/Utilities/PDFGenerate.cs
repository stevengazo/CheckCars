

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
            using (MemoryStream ms = new MemoryStream())
            {
                PdfWriter writer = new PdfWriter(ms);
                PdfDocument pdf = new PdfDocument(writer);
                Document document = new Document(pdf);


                Paragraph header = new Paragraph(nameof(EntryExitReport))
                    .SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER)
                    .SetFontSize(20);

                document.Close();
                return ms.ToArray();



            }
        }
    }
}

