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
        new(ContentStreamComposite, ContentStreamComposite);

    public static readonly PdfDictionaryParser InlineImageDictionaryParser =
        new(new ExpandSynonymsParser(new InlineImageNameParser(),
            new Dictionary<PdfObject, PdfObject>()
            {
                {PdfTokenValues.InlineImageDictionaryTerminator, PdfTokenValues.DictionaryTerminator},
                {InlineImageFieldName.BPC, KnownNames.BitsPerComponent},
                {InlineImageFieldName.CS, KnownNames.ColorSpace},
                {InlineImageFieldName.D, KnownNames.Decode},
                {InlineImageFieldName.DP, KnownNames.DecodeParms},
                {InlineImageFieldName.F, KnownNames.Filter},
                {InlineImageFieldName.H, KnownNames.Height},
                {InlineImageFieldName.IM, KnownNames.ImageMask},
                {InlineImageFieldName.I, KnownNames.Interpolate},
                {InlineImageFieldName.L, KnownNames.Length},
                {InlineImageFieldName.W, KnownNames.Width}
            }), 
            new ExpandSynonymsParser(ContentStreamComposite,
                new Dictionary<PdfObject, PdfObject>()
                {
                    {InlineImageFilterName.AHx, FilterName.ASCIIHexDecode},
                    {InlineImageFilterName.A85, FilterName.ASCII85Decode},
                    {InlineImageFilterName.LZW, FilterName.LZWDecode},
                    {InlineImageFilterName.Fl, FilterName.FlateDecode},
                    {InlineImageFilterName.RL, FilterName.RunLengthDecode},
                    {InlineImageFilterName.CCF, FilterName.CCITTFaxDecode},
                    {InlineImageFilterName.DCT, FilterName.DCTDecode},
                    {InlineImageColorSpaceName.G, ColorSpaceName.DeviceGray},
                    {InlineImageColorSpaceName.RGB, ColorSpaceName.DeviceRGB},
                    {InlineImageColorSpaceName.CMYK, ColorSpaceName.DeviceCMYK},
                    {InlineImageColorSpaceName.I, ColorSpaceName.Indexed},
                }));
}