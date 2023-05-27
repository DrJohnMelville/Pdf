using System.Numerics;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using Melville.Pdf.Model.Renderers.DocumentRenderers;
using Melville.Pdf.Model.Renderers.FontRenderings;

namespace Melville.Pdf.TextExtractor;

public static class TextExtractorFacade
{
    public static async ValueTask<string> PageTextAsync(
        this DocumentRenderer renderer, int oneBasedPageNumber)
    {
        var target = new ConcatenatingTextTarget();
        await renderer.RenderPageToAsync(oneBasedPageNumber,
            (_,__)=>new ExtractTextRender(target));
        return target.AllText();
    }
}

public class ConcatenatingTextTarget: IExtractedTextTarget
{
    private readonly StringBuilder target = new StringBuilder();

    public void RenderText(string text, int page, Vector2 start, Vector2 end, double size, IRealizedFont font)
    {
        target.Append(text);
    }

    public string AllText() => target.ToString();
}
