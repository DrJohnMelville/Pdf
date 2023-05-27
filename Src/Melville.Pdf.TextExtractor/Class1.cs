using Melville.Pdf.Model.Renderers.DocumentRenderers;

namespace Melville.Pdf.TextExtractor;

public static class TextExtractorFacade
{
    public static async ValueTask<string> PageTextAsync(
        this DocumentRenderer renderer, int oneBasedPageNumber) =>
        $"rendered page {oneBasedPageNumber}";
}