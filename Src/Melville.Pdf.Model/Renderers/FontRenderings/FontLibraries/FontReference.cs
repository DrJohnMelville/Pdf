using System.IO;
using System.Threading.Tasks;
using Melville.Parsing.AwaitConfiguration;
using Melville.Pdf.Model.Renderers.FontRenderings.FreeType;

namespace Melville.Pdf.Model.Renderers.FontRenderings.FontLibraries;

public record FontReference(string FileName, int Index)
{
    public async ValueTask<IRealizedFont> ReaiizeUsing(FreeTypeFontFactory factory)
    {
        await using var stream = File.Open(FileName, FileMode.Open, FileAccess.Read, FileShare.Read);
        return await factory.FromCSharpStream(stream, Index).CA();

    }
}