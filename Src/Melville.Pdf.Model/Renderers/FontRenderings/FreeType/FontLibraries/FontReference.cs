using System.IO;
using System.Threading.Tasks;
using Melville.Parsing.AwaitConfiguration;

namespace Melville.Pdf.Model.Renderers.FontRenderings.FreeType.FontLibraries;

public record FontReference(string FileName, int Index)
{
    public async ValueTask<IRealizedFont> ReaiizeUsing(FreeTypeFontFactory factory)
    {
        await using var stream = File.Open(FileName, FileMode.Open, FileAccess.Read, FileShare.Read);
        return await factory.FromCSharpStream(stream, Index).CA();

    }
}