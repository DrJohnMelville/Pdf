using System.IO.Pipelines;
using System.Linq;
using System.Threading.Tasks;
using Melville.Parsing.Streams;
using Melville.Pdf.LowLevel.Model.CharacterEncoding;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.Model.Renderers.FontRenderings.Type3;
using SixLabors.Fonts;

namespace Melville.Pdf.Model.Renderers.FontRenderings.OpenType;

public static class OpenTypeFontFactory
{
    public static IRealizedFont SystemFont(byte[] name, double size, IFontTarget target,
        IByteToUnicodeMapping mapping, bool bold, bool oblique)
    {
        return new OpenTypeRealizedFont(target, size, mapping,
            FindFontFamily(name).CreateFont((float)size, ComputeFontStyle(bold, oblique) ));
    }

    private static FontStyle ComputeFontStyle(bool bold, bool oblique) => (bold, oblique) switch
    {
        (false, true) => FontStyle.Italic,
        (true, true) => FontStyle.BoldItalic,
        (true, false) => FontStyle.Bold,
        _ => FontStyle.Regular
    };
    
    private static FontFamily FindFontFamily(byte[] fontName)
    {
        int currentLen = -1;
        FontFamily? result = null;
        foreach (var family in SystemFonts.Families)
        {
            var len = fontName.CommonPrefixLength(family.Name);
            if (len > currentLen || 
                (len == currentLen && family.Name.Length < (result?.Name.Length??1000)))
            {
                currentLen = len;
                result = family;
            }
        }
        return result ?? SystemFonts.Families.First();
    }

    public static async Task<IRealizedFont> FromStream(PdfStream pdfStream, double size, IFontTarget target,
        IByteToUnicodeMapping mapping)
    {
        var source = await pdfStream.StreamContentAsync();
        var decodedSource = new MultiBufferStream();
        await source.CopyToAsync(decodedSource);
        var fontCollection = new FontCollection();
        fontCollection.Install(decodedSource.CreateReader());
        var font = fontCollection.Families.First().CreateFont((float)size, FontStyle.Regular);
        return new OpenTypeRealizedFont(target, size, mapping, font);
    }
}