using System.Threading.Tasks;
using Melville.Parsing.Streams;
using Melville.Pdf.LowLevel;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Document;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Parsing.FileParsers;
using Melville.Pdf.LowLevel.Parsing.ObjectParsers;
using Melville.Pdf.LowLevel.Parsing.ParserContext;

namespace Melville.Pdf.DataModelTests.ParsingTestUtils;

public static class TestParser
{
    public static ValueTask<PdfIndirectObject> ParseValueObjectAsync(this string s) =>
        ParseValueObjectAsync(AsParsingSource(s));

    public static async ValueTask<T> ParseValueObjectAsync<T>(this string s) =>
        await (await ParseValueObjectAsync(AsParsingSource(s))).LoadValueAsync<T>();

    public static ValueTask<PdfIndirectObject> ParseValueObjectAsync(this byte[] bytes) => 
        ParseValueObjectAsync(AsParsingSource(bytes));

    internal static async ValueTask<PdfIndirectObject> ParseValueObjectAsync(this ParsingFileOwner source, long position = 0)
    {
        var reader = await source.RentReaderAsync(position);
        return await new RootObjectParser(reader).ParseAsync();
    }

    public static ValueTask<PdfDirectObject> ParseRootObjectAsync(this string s) =>
        ParseRootObjectAsync(AsParsingSource(s));

    public static ValueTask<PdfDirectObject> ParseRootObjectAsync(this byte[] bytes) => 
        ParseRootObjectAsync(AsParsingSource(bytes));

    internal static async ValueTask<PdfDirectObject> ParseRootObjectAsync(this ParsingFileOwner source, long position = 0)
    {
        var reader = await source.RentReaderAsync(position);
        return await new RootObjectParser(reader).ParseTopLevelObjectAsync();
    }

    internal static ParsingFileOwner AsParsingSource(this string str) =>
        AsParsingSource(str.AsExtendedAsciiBytes());
    internal static ParsingFileOwner AsParsingSource(this byte[] bytes) => 
        new(new OneCharAtATimeStream(bytes), NullPasswordSource.Instance);
        
    public static ValueTask<PdfLoadedLowLevelDocument> ParseDocumentAsync(this string str, int sizeHint = 1024) =>
        RandomAccessFileParser.ParseAsync(str.AsParsingSource(), sizeHint);
    
    public static ValueTask<PdfLoadedLowLevelDocument> ParseWithPasswordAsync(
        this string str, string password, PasswordType type) =>
        new PdfLowLevelReader(new ConstantPasswordSource(type, password)).ReadFromAsync(str.AsExtendedAsciiBytes());
}