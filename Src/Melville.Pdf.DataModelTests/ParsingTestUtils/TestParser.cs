using System.IO;
using System.Threading.Tasks;
using Melville.Parsing.AwaitConfiguration;
using Melville.Parsing.Streams;
using Melville.Pdf.LowLevel;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Document;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Model.Objects2;
using Melville.Pdf.LowLevel.Model.Primitives;
using Melville.Pdf.LowLevel.Parsing.FileParsers;
using Melville.Pdf.LowLevel.Parsing.ObjectParsers;
using Melville.Pdf.LowLevel.Parsing.ObjectParsers2;
using Melville.Pdf.LowLevel.Parsing.ParserContext;

namespace Melville.Pdf.DataModelTests.ParsingTestUtils;

public static class TestParser
{
    public static Task<PdfObject> ParseObjectAsync(this string s) =>
        ParseObjectAsync(AsParsingSource(s));

    public static Task<PdfObject> ParseObjectAsync(this byte[] bytes) => 
        ParseObjectAsync(AsParsingSource(bytes));

    internal static async Task<PdfObject> ParseObjectAsync(this ParsingFileOwner source, long position = 0)
    {
        var reader = await source.RentReaderAsync(position);
        return await PdfParserParts.Composite.ParseAsync(reader);
    }

    public static ValueTask<PdfIndirectValue> ParseValueObjectAsync(this string s) =>
        ParseValueObjectAsync(AsParsingSource(s));

    public static ValueTask<PdfIndirectValue> ParseValueObjectAsync(this byte[] bytes) => 
        ParseValueObjectAsync(AsParsingSource(bytes));

    internal static async ValueTask<PdfIndirectValue> ParseValueObjectAsync(this ParsingFileOwner source, long position = 0)
    {
        var reader = await source.RentReaderAsync(position);
        return await new RootObjectParser(reader).ParseAsync();
    }

    public static ValueTask<PdfDirectValue> ParseRootObjectAsync(this string s) =>
        ParseRootObjectAsync(AsParsingSource(s));

    public static ValueTask<PdfDirectValue> ParseRootObjectAsync(this byte[] bytes) => 
        ParseRootObjectAsync(AsParsingSource(bytes));

    internal static async ValueTask<PdfDirectValue> ParseRootObjectAsync(this ParsingFileOwner source, long position = 0)
    {
        var reader = await source.RentReaderAsync(position);
        return await new RootObjectParser(reader).ParseTopLevelObject();
    }

    internal static ParsingFileOwner AsParsingSource(this string str, 
        IIndirectObjectResolver? indirectObjectResolver =null) =>
        AsParsingSource(str.AsExtendedAsciiBytes(), indirectObjectResolver);
    internal static ParsingFileOwner AsParsingSource(this byte[] bytes, 
        IIndirectObjectResolver? indirectObjectResolver =null) => 
        new(new OneCharAtATimeStream(bytes), NullPasswordSource.Instance, 
            indirectObjectResolver?? new IndirectObjectResolver());
        
    public static ValueTask<PdfLoadedLowLevelDocument> ParseDocumentAsync(this string str, int sizeHint = 1024) =>
        RandomAccessFileParser.ParseAsync(str.AsParsingSource(), sizeHint);
    
    public static ValueTask<PdfLoadedLowLevelDocument> ParseWithPasswordAsync(
        this string str, string password, PasswordType type) =>
        new PdfLowLevelReader(new ConstantPasswordSource(type, password)).ReadFromAsync(str.AsExtendedAsciiBytes());
}