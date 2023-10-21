using Melville.INPC;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Document;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Writers.Builder;

namespace Melville.Pdf.FormReader.AcroForms;

internal partial class AcroPdfForm : IPdfForm
{
    [FromConstructor] private readonly PdfLowLevelDocument doc;
    [FromConstructor] public IReadOnlyList<IPdfFormField> Fields { get; }
    public PdfLowLevelDocument CreateModifiedDocument()
    {
        var ret = new ModifyableLowLevelDocument(doc);
        WriteChangedFields(ret);
        return ret;
    }

    private void WriteChangedFields(ICanReplaceObjects target)
    {
        foreach (var field in Fields.OfType<AcroFormField>())
        {
            field.WriteChangeTo(target);
        }
    }
}

internal partial class AcroFormField: IPdfFormField
{
    [FromConstructor] public string Name { get; }
    public PdfDirectObject Value { get; set; }
    [FromConstructor] private readonly PdfDirectObject originalValue;
    [FromConstructor] private readonly PdfIndirectObject indirectRef;
    [FromConstructor] private readonly PdfDictionary sourceDictionary;

    partial void OnConstructed()
    {
        Value = originalValue;
    }

    public void WriteChangeTo(ICanReplaceObjects target)
    {
        if (Value.Equals(originalValue)) return;
        target.ReplaceReferenceObject(indirectRef, 
            new DictionaryBuilder(sourceDictionary.RawItems)
                .WithItem(KnownNames.V, Value)
                .AsDictionary());
    }
}