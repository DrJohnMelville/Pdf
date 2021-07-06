using System.Diagnostics.CodeAnalysis;
using Melville.Pdf.LowLevel.Model;
using Melville.Pdf.LowLevel.Parsing.NameParsing;

namespace Melville.Pdf.DataModelTests.ParsingTestUtils
{
    public static class TestParser
    {
        public static bool ParseAs(this string s) =>
            s.ParseAs(out PdfObject? _);
        public static bool ParseAs<T>(this string s, [NotNullWhen(true)]out T? obj) where T : PdfObject
        {
            var seq = s.AsSequenceReader();
            var ret = new PdfCompositeObjectParser().TryParse(ref seq, out var parsed);
            obj = ret ? (T?) parsed : null;
            return ret;
        }
    }
}