using Melville.INPC;
using Melville.Parsing.AwaitConfiguration;
using Melville.Pdf.LowLevel.Model.Document;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Writers.Builder;

namespace Melville.Pdf.FormReader.AcroForms;

internal partial class AcroPdfForm : IPdfForm
{
    [FromConstructor] public readonly PdfLowLevelDocument document;
    [FromConstructor] public IReadOnlyList<IPdfFormField> Fields { get; }
    [FromConstructor] private readonly PdfDirectObject formAppearanceString;

    public async ValueTask<PdfLowLevelDocument> CreateModifiedDocumentAsync()
    {
        var ret = new ModifyableLowLevelDocument(document );
        await WriteChangedFieldsAsync(ret).CA();
        return ret;
    }

    private async ValueTask WriteChangedFieldsAsync(ICanReplaceObjects target)
    {
        foreach (var field in Fields.OfType<AcroFormField>())
        {
            await field.WriteChangeTo(target, formAppearanceString);
        }
    }
}