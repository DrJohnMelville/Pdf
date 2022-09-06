using System.IO;
using System.Threading.Tasks;
using Melville.Parsing.Streams;
using Melville.Pdf.LowLevel;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Document;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Model.Primitives;
using Melville.Pdf.LowLevel.Parsing.FileParsers;
using Melville.Pdf.LowLevel.Parsing.ObjectParsers;
using Melville.Pdf.LowLevel.Parsing.ParserContext;

namespace Melville.Pdf.DataModelTests.ParsingTestUtils;

public static class TestParser
{
    public static Task<PdfObject> ParseObjectAsync(this string s) =>
        ParseObjectAsync(AsParsingSource(s));

    public static Task<PdfObject> ParseObjectAsync(this byte[] bytes) => 
        ParseObjectAsync(AsParsingSource(bytes));

    public static async Task<PdfObject> ParseObjectAsync(this ParsingFileOwner source, long position = 0)
    {
        var reader = await source.RentReader(position);
        return await PdfParserParts.Composite.ParseAsync(reader);
    }

    public static ParsingFileOwner AsParsingSource(this string str, 
        IIndirectObjectResolver? indirectObjectResolver =null) =>
        AsParsingSource(str.AsExtendedAsciiBytes(), indirectObjectResolver);
    public static ParsingFileOwner AsParsingSource(this byte[] bytes, 
        IIndirectObjectResolver? indirectObjectResolver =null) => 
        new(new OneCharAtAtimeStream(bytes), null, indirectObjectResolver);
        
    public static ValueTask<PdfLoadedLowLevelDocument> ParseDocumentAsync(this string str, int sizeHint = 1024) =>
        RandomAccessFileParser.Parse(str.AsParsingSource(), sizeHint);
    
    public static ValueTask<PdfLoadedLowLevelDocument> ParseWithPassword(
        this string str, string password, PasswordType type) =>
        new PdfLowLevelReader(new ConstantPasswordSource(type, password)).ReadFrom(str.AsExtendedAsciiBytes());
}