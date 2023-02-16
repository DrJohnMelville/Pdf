using System.IO;
using System.Threading.Tasks;
using Melville.Parsing.AwaitConfiguration;
using Melville.Pdf.Model.Renderers.FontRenderings.DefaultFonts;
using Melville.Pdf.Model.Renderers.FontRenderings.FreeType;

namespace Melville.Pdf.Model.Renderers.FontRenderings.FontLibraries;

internal record FontReference(string FileName, int Index)
{
    private FileStream FontAsStream() => 
        File.Open(FileName, FileMode.Open, FileAccess.Read, FileShare.Read);

    public DefaultFontReference AsDefaultFontReference() => new(FontAsStream(), Index);

}