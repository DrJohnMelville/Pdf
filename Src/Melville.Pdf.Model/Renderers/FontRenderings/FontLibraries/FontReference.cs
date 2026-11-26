using System.IO;
using Melville.Pdf.Model.Renderers.FontRenderings.DefaultFonts;
using Melville.StreamInterfaces;
using Melville.StreamInterfaces.Files;

namespace Melville.Pdf.Model.Renderers.FontRenderings.FontLibraries;

internal record FontReference(string FileName, int Index)
{
    private IAsyncReader FontAsStream() =>
        FileIStreamFactory.File(FileName, FileMode.Open, FileShare.Read).Reader();

    public DefaultFontReference AsDefaultFontReference() => new(FontAsStream(), Index);

}