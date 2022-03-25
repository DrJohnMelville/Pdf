using Melville.Pdf.LowLevel.Model.Objects;

namespace Melville.Pdf.Model.OptionalContent;

public sealed class AllOptionalContentVisible : IOptionalContentState
{
    public static readonly AllOptionalContentVisible Instance = new();
    private AllOptionalContentVisible() { }
    public bool IsGroupVisible(PdfDictionary dictionary) => true;
}