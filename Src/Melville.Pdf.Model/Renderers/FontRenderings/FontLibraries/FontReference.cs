using System.IO;
using Melville.Pdf.Model.Renderers.FontRenderings.DefaultFonts;

namespace Melville.Pdf.Model.Renderers.FontRenderings.FontLibraries;

internal record FontReference(string FileName, int Index)
{
    private FileStream FontAsStream() => 
        File.Open(FileName, FileMode.Open, FileAccess.Read, FileShare.Read);

    public DefaultFontReference AsDefaultFontReference() => new(FontAsStream(), Index);

}