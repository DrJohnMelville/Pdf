using System.Collections.Generic;
using Melville.Pdf.LowLevel.Model.ContentStreams;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Parsing.StringParsing;

namespace Melville.Pdf.LowLevel.Parsing.ObjectParsers;

public static class PdfParserParts
{
    public static readonly IPdfObjectParser ContentStreamComposite = new PdfCompositeObjectParserBase(); 
    public static readonly IPdfObjectParser Composite = new PdfCompositeObjectParser();
    public static readonly IPdfObjectParser HexString = new HexStringParser();
    public static readonly IPdfObjectParser SyntaxString = new SyntaxStringParser();
    public static readonly PdfArrayParser PdfArray = new();
    public static readonly PdfDictionaryAndStreamParser dictionaryAndStream = new();
    public static readonly NumberParser Number = new();
    public static readonly NameParser Names = new();
    public static readonly LiteralTokenParser TrueParser = new(PdfBoolean.True);
    public static readonly LiteralTokenParser FalseParser = new(PdfBoolean.False);
    public static readonly LiteralTokenParser NullParser = new(PdfTokenValues.Null);
    public static readonly LiteralTokenParser ArrayTermination = new(PdfTokenValues.ArrayTerminator);
    public static readonly LiteralTokenParser DictionatryTermination = new(PdfTokenValues.DictionaryTerminator);
// order is important, declarations after this comment rely on others.
    public static readonly PdfDictionaryParser Dictionary = 
        new(ContentStreamComposite, Composite, PdfDictionaryParser.InlineImagePrefix);
    public static readonly IndirectObjectParser Indirects = new(Number);
    public static readonly PdfDictionaryParser EmbeddedDictionaryParser =
        new(ContentStreamComposite, ContentStreamComposite, PdfDictionaryParser.InlineImagePrefix);

    public static readonly PdfDictionaryParser InlineImageDictionaryParser =
        new PdfDictionaryParser(new ExpandSynonymsParser(
            new InlineImageNameParser(),
            new Dictionary<PdfObject, PdfObject>()
            {
                {PdfTokenValues.InlineImageDictionaryTerminator, PdfTokenValues.DictionaryTerminator}
            }), ContentStreamComposite, PdfDictionaryParser.InlineImagePrefix);
}