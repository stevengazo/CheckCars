using PdfSharp.Fonts;
using System.IO;
using System.Reflection;

namespace CheckCars.Utilities
{
    public class FontResolver : IFontResolver
    {
        public byte[] GetFont(string faceName)
        {
            if (faceName == "ArialRegular")
            {
                // Obtén el ensamblado actual para cargar el recurso embebido
                var assembly = Assembly.GetExecutingAssembly();

                // Cambia "CheckCars.Resources.Fonts.ARIAL.ttf" al espacio de nombres correcto
                var resourcePath = "CheckCars.Resources.Fonts.Arial.ttf";

                // Cargar la fuente como un recurso embebido
                using var resourceStream = assembly.GetManifestResourceStream(resourcePath);
                if (resourceStream != null)
                {
                    using var memoryStream = new MemoryStream();
                    resourceStream.CopyTo(memoryStream);
                    return memoryStream.ToArray();
                }
            }
            return null;
        }

        public string[] GetFontNames() => new string[] { "Arial" };

        public FontResolverInfo? ResolveTypeface(string familyName, bool bold, bool italic)
        {
            if (familyName.Equals("ArialRegular", StringComparison.OrdinalIgnoreCase))
            {
                return new FontResolverInfo("ArialRegular", bold, italic);
            }
            return null;
        }
    }
}
