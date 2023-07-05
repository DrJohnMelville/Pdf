using System.Collections.Generic;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Postscript.Interpreter.Tokenizers;

namespace Melville.Pdf.LowLevel.Parsing.ObjectParsers;

internal static class PdfParserParts
{
    public static readonly IPdfObjectParser ContentStreamComposite = new PdfCompositeObjectParserBase(); 
    public static readonly IPdfObjectParser Composite = new PdfCompositeObjectParser();
    public static readonly IPdfObjectParser HexString = new StringDecoderParser<HexStringDecoder, byte>();
    public static readonly IPdfObjectParser SyntaxString = new StringDecoderParser<SyntaxStringDecoder, int>();
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
        new(ContentStreamComposite, Composite);
    public static readonly IndirectObjectParser Indirects = new(Number);
    public static readonly PdfDictionaryParser EmbeddedDictionaryParser =
        new(ContentStreamComposite, ContentStreamComposite); }