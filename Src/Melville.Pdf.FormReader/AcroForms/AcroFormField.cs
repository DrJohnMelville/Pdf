using Melville.INPC;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Writers.Builder;

namespace Melville.Pdf.FormReader.AcroForms;

internal partial class AcroFormField: IPdfFormField
{
    [FromConstructor] public string Name { get; }
    public PdfDirectObject Value { get; set; }
    [FromConstructor] private readonly PdfDirectObject originalValue;
    [FromConstructor] private readonly PdfIndirectObject indirectRef;
    [FromConstructor] protected readonly PdfDictionary sourceDictionary;

    partial void OnConstructed()
    {
        Value = originalValue;
    }

    public ValueTask WriteChangeTo(ICanReplaceObjects target, PdfDirectObject formAppearanceString)
    {
        if (Value.Equals(originalValue)) return ValueTask.CompletedTask;
        var builder = new DictionaryBuilder(SourceWithoutIFields())
            .WithItem(KnownNames.V, Value);
        if (builder.TryGetValue(KnownNames.AS, out _))
            builder.WithItem(KnownNames.AS, Value);
        target.ReplaceReferenceObject(indirectRef, 
            builder.AsDictionary());

        return UpdateAppearance(target, formAppearanceString);
    }

    private IEnumerable<KeyValuePair<PdfDirectObject, PdfIndirectObject>> SourceWithoutIFields() =>
        sourceDictionary.RawItems
            .Where(i=>!i.Key.Equals(KnownNames.I));

    protected virtual ValueTask UpdateAppearance(ICanReplaceObjects target, PdfDirectObject formAppearanceString)
    {
        return ValueTask.CompletedTask;
    }
}