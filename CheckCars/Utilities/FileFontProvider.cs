
using PdfSharpCore.Drawing;
using PdfSharpCore.Fonts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace CheckCars.Utilities
{
    internal class FileFontProvider : IFontResolver
    {
                public string DefaultFontName => "OpenSansRegular";

        // Este método devuelve el contenido en bytes de la fuente solicitada.
        public byte[]? GetFont(string faceName)
        {
            try
            {
                // Resolución de la fuente según el nombre de la cara
                var fontStream = GetFontFromResource($"{faceName}.ttf");

                // Leer el contenido del Stream de la fuente en un array de bytes
                using (var memoryStream = new MemoryStream())
                {
                    fontStream.CopyTo(memoryStream);
                    return memoryStream.ToArray();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al cargar la fuente: {ex.Message}");
                return null;
            }
        }

        // Resolución de la fuente dependiendo de los estilos (negrita, cursiva).
        public FontResolverInfo? ResolveTypeface(string familyName, bool bold, bool italic)
        {
            // Dependiendo del estilo, resolvemos el nombre de la fuente.
            if (familyName == "OpenSans")
            {
                if (bold && italic)
                    return new FontResolverInfo("OpenSans-Semibold", 0);  // Puedes usar la versión semibold en este caso
                else if (bold)
                    return new FontResolverInfo("OpenSans-Semibold", 0);
                else if (italic)
                    return new FontResolverInfo("OpenSans-Italic", 0);  // O alguna versión cursiva
                else
                    return new FontResolverInfo("OpenSans-Regular", 0);
            }

            // Si no encontramos la fuente, devolvemos null
            return null;
        }

        // Método para cargar la fuente desde los recursos.
        private Stream GetFontFromResource(string resourceName)
        {
            // Accedemos a los recursos de la aplicación
            var assembly = typeof(App).Assembly;
            var fontStream = assembly.GetManifestResourceStream($"CheckCars.Resources.Fonts.{resourceName}");

            if (fontStream == null)
            {
                throw new FileNotFoundException($"Font file '{resourceName}' not found.");
            }

            return fontStream;
        }
    }

}

