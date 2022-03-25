using System.Threading.Tasks;
using Melville.Pdf.LowLevel.Model.Objects;

namespace Melville.Pdf.Model.OptionalContent;

public sealed class AllOptionalContentVisible : IOptionalContentState
{
    public static readonly AllOptionalContentVisible Instance = new();
    private AllOptionalContentVisible() { }
    public ValueTask<bool> IsGroupVisible(PdfDictionary? dictionary) => new(false);
}