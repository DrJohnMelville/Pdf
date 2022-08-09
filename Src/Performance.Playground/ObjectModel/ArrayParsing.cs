using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using Melville.Pdf.DataModelTests.ParsingTestUtils;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Parsing.ObjectParsers;
using Melville.Pdf.LowLevel.Parsing.ParserContext;

namespace Performance.Playground.ObjectModel;

[MemoryDiagnoser()]
public class ArrayParsing {
        private static readonly byte[] source = @"[
/Index /Index /Index /Index /Index /Index /Index /Index /Index /Index /Index /Index /Index /Index /Index 
]".AsExtendedAsciiBytes();
        
        private static async ValueTask<PdfObject> RunTest(IPdfObjectParser parser)
        {
                PdfObject ret = PdfArray.Empty;
                for (int i = 0; i < 1000; i++)
                {
                        ret = await parser.ParseAsync( await source.AsParsingSource().RentReader(0));
                }
                return ret;
        }

        [Benchmark]
        public ValueTask<PdfObject> ParseNew() => RunTest(new PdfArrayParser());
}