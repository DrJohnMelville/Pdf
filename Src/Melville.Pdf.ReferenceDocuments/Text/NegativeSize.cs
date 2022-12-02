using System.Numerics;
using Melville.Pdf.LowLevel.Model.Primitives;
using Melville.Pdf.LowLevel.Writers.ContentStreams;

namespace Melville.Pdf.ReferenceDocuments.Text;

public class NegativeSize: FontDefinitionTest
{
    
    public NegativeSize() : base("Has negative font size")
    {
        FontSize = -70;
    }

    protected override PdfObject CreateFont(ILowLevelDocumentCreator arg)
    {
        return new DictionaryBuilder()
            .WithItem(KnownNames.Type, KnownNames.Font)
            .WithItem(KnownNames.Subtype, KnownNames.Type1)
            .WithItem(KnownNames.BaseFont, BuiltInFontName.Helvetica)
            .AsDictionary();
    }

    protected override ValueTask DoPaintingAsync(ContentStreamWriter csw)
    {
        csw.ModifyTransformMatrix(Matrix3x2.CreateTranslation(300,70));
        return base.DoPaintingAsync(csw);
    }
}