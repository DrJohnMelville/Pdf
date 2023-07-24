using Melville.Pdf.LowLevel.Model.Objects2;
using Melville.Pdf.LowLevel.Model.Primitives;

namespace Melville.Pdf.ReferenceDocuments.Text.Encoding;

public class EncodingWithDiferences : FontDefinitionTest
{

    public EncodingWithDiferences() : base("Uses an encoding dictionary to make a different font array")
    {
        TextToRender = "ABC DE";
    }

    protected override PdfDirectValue CreateFont(IPdfObjectCreatorRegistry arg)
    {
        var enc = arg.Add(new ValueDictionaryBuilder()
            .WithItem(KnownNames.DifferencesTName, new PdfValueArray(
                65,
                PdfDirectValue.CreateName("AE"),
                PdfDirectValue.CreateName("Adieresis"),
                PdfDirectValue.CreateName("ff")
            ))
            .AsDictionary());
        return new ValueDictionaryBuilder()
            .WithItem(KnownNames.TypeTName, KnownNames.FontTName)
            .WithItem(KnownNames.SubtypeTName, KnownNames.Type1TName)
            .WithItem(KnownNames.BaseFontTName, (PdfDirectValue)BuiltInFontName.Courier)
            .WithItem(KnownNames.EncodingTName, enc)
            .AsDictionary();
    }
}