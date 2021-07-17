using System.Threading.Tasks;
using Melville.Pdf.LowLevel.Model.LowLevel;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Model.Primitives;
using Melville.Pdf.LowLevel.Parsing.FileParsers;
using Melville.Pdf.LowLevel.Parsing.ObjectParsers;
using Melville.Pdf.LowLevel.Parsing.ParserContext;

namespace Melville.Pdf.DataModelTests.ParsingTestUtils
{
    public static class TestParser
    {
        public static Task<PdfObject> ParseObjectAsync(this string s) =>
            ParseObjectAsync(AsParsingSource(s));

        public static Task<PdfObject> ParseObjectAsync(this byte[] bytes) => 
            ParseObjectAsync(AsParsingSource(bytes));

        public static Task<PdfObject> ParseObjectAsync(this ParsingSource source) => 
            source.RootObjectParser.ParseAsync(source);

        public static ParsingSource AsParsingSource(this string str, 
            IIndirectObjectResolver? indirectObjectResolver =null) =>
            AsParsingSource((str + " /%This simulates an end tag\r\n").AsExtendedAsciiBytes(), indirectObjectResolver);
        public static ParsingSource AsParsingSource(this byte[] bytes, 
            IIndirectObjectResolver? indirectObjectResolver =null) => 
            new(new OneCharAtAtimeStream(bytes), new PdfCompositeObjectParser(), indirectObjectResolver);
        
        public static Task<PdfLowLevelDocument> ParseDocumentAsync(this string str, int sizeHint = 1024) => 
             RandomAccessFileParser.Parse(str.AsParsingSource(), sizeHint);
    }
}