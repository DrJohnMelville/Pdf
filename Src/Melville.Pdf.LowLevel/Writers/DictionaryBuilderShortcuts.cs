using Melville.Pdf.LowLevel.Model.Objects;

namespace Melville.Pdf.LowLevel.Writers;

public static class DictionaryBuilderShortcuts
{
    public static DictionaryBuilder WithItem(this in DictionaryBuilder builder, PdfName name, int item) =>
        builder.WithItem(name, new PdfInteger(item));

    public static DictionaryBuilder WithItem(this in DictionaryBuilder builder, PdfName name, double item) =>
        builder.WithItem(name, new PdfDouble(item));
    
    public static DictionaryBuilder WithItem(this in DictionaryBuilder builder, PdfName name, bool item) =>
        builder.WithItem(name, item?PdfBoolean.True:PdfBoolean.False);
    
}