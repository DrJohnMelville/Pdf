using System.Threading.Tasks;
using Melville.Pdf.LowLevel.Model.Objects;

namespace Melville.Pdf.Model.OptionalContent;

public static class OptionalContentPropertiesParser
{
    public static ValueTask<IOptionalContentState> ParseAsync(PdfDictionary? oCProperties) =>
        new(AllOptionalContentVisible.Instance);
}