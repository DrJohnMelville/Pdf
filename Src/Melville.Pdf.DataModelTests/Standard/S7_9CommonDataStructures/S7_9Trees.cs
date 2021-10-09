using System.Linq;
using System.Threading.Tasks;
using Melville.Pdf.DataModelTests.ParsingTestUtils;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Model.Primitives;
using Melville.Pdf.LowLevel.Model.Wrappers;
using Melville.Pdf.LowLevel.Parsing.ParserContext;
using Melville.Pdf.LowLevel.Writers.Builder;
using Xunit;

namespace Melville.Pdf.DataModelTests.Standard.S7_9CommonDataStructures
{
    public class S7_9Trees
    {
        private readonly ILowLevelDocumentCreator builder = new LowLevelDocumentCreator();

        private PdfDictionary CreateNumberTree(int count)
        {
            return builder.CreateTree(10,
                Enumerable.Range(1, count)
                    .Reverse()
                    .Select(i => (new PdfInteger(i), (PdfObject)new PdfInteger(10 * i))));
        }

        [Fact]
        public async Task CreateTrivialNumberTree()
        {
            var result = CreateNumberTree(5);
            var array = await result.GetAsync<PdfArray>(KnownNames.Nums);
            for (int i = 0; i < 5; i++)
            {
                Assert.Equal(1+i, (PdfNumber)await array[2*i]);
                Assert.Equal(10*(1+i), (PdfNumber)await array[2*i+1]);
            }
        }

        [Fact]
        public async Task TwoLevelTree()
        {
            var result = CreateNumberTree(50);
            var array = await result.GetAsync<PdfArray>(KnownNames.Kids);
            builder.Add(result);
            Assert.Equal(5, array.Count);
            var secondNode = (PdfDictionary)await array[1];
            Assert.Equal(11, (PdfNumber)await (await secondNode.GetAsync<PdfArray>(KnownNames.Nums))[0]);
            Assert.Equal(110, (PdfNumber)await (await secondNode.GetAsync<PdfArray>(KnownNames.Nums))[1]);
            Assert.Equal(20, (PdfNumber)await (await secondNode.GetAsync<PdfArray>(KnownNames.Nums))[18]);
            Assert.Equal(200, (PdfNumber)await (await secondNode.GetAsync<PdfArray>(KnownNames.Nums))[19]);
            Assert.Equal(11, (PdfNumber) await (await secondNode.GetAsync<PdfArray>(KnownNames.Limits))[0]);
            Assert.Equal(20, (PdfNumber) await (await secondNode.GetAsync<PdfArray>(KnownNames.Limits))[1]);
            var docAsString = await builder.AsStringAsync();
            Assert.Contains("1 0 obj <</Nums[1 10", docAsString);
            Assert.Contains("6 0 obj <</Kids[1 0 R 2 0 R 3 0 R 4 0 R 5 0 R]>> endobj", docAsString);
        }
        [Fact]
        public async Task ThreeLevelTree()
        {
            var result = CreateNumberTree(500);
            var array = await result.GetAsync<PdfArray>(KnownNames.Kids);
            builder.Add(result);
            Assert.Equal(5, array.Count);
            var secondNode = await array.GetAsync<PdfDictionary>(1);
            Assert.Equal(101, (PdfNumber) await (await secondNode.GetAsync<PdfArray>(KnownNames.Limits))[0]);
            Assert.Equal(200, (PdfNumber) await (await secondNode.GetAsync<PdfArray>(KnownNames.Limits))[1]);
            var thirdNode = await (await secondNode.GetAsync<PdfArray>(KnownNames.Kids)).GetAsync<PdfDictionary>(1);
            Assert.Equal(111, (PdfNumber)await (await thirdNode.GetAsync<PdfArray>(KnownNames.Nums))[0]);
            Assert.Equal(1110, (PdfNumber)await (await thirdNode.GetAsync<PdfArray>(KnownNames.Nums))[1]);
            Assert.Equal(120, (PdfNumber)await (await thirdNode.GetAsync<PdfArray>(KnownNames.Nums))[18]);
            Assert.Equal(1200, (PdfNumber)await (await thirdNode.GetAsync<PdfArray>(KnownNames.Nums))[19]);
            Assert.Equal(111, (PdfNumber) await (await thirdNode.GetAsync<PdfArray>(KnownNames.Limits))[0]);
            Assert.Equal(120, (PdfNumber) await (await thirdNode.GetAsync<PdfArray>(KnownNames.Limits))[1]);
        }

        [Fact]
        public async Task CreateTrivialNameTree()
        {
            var result = builder.CreateTree(10,
                (PdfString.CreateAscii("A"), PdfString.CreateAscii("Alpha")),
                (PdfString.CreateAscii("C"), PdfString.CreateAscii("Charlie")),
                (PdfString.CreateAscii("B"), PdfString.CreateAscii("Bravo"))
            );
            var array = await result.GetAsync<PdfArray>(KnownNames.Names);
            Assert.Equal("A", ((PdfString)await array[0]).AsAsciiString());
            Assert.Equal("Alpha", ((PdfString)await array[1]).AsAsciiString());
            Assert.Equal("B", ((PdfString)await array[2]).AsAsciiString());
            Assert.Equal("Bravo", ((PdfString)await array[3]).AsAsciiString());
            Assert.Equal("C", ((PdfString)await array[4]).AsAsciiString());
            Assert.Equal("Charlie", ((PdfString)await array[5]).AsAsciiString());
            
        }

        [Theory]
        [InlineData(1,10)]
        [InlineData(213,2130)]
        [InlineData(500,5000)]
        public async Task IndexerTest(int key, int value)
        {
            var tree = new PdfTree<PdfNumber>(CreateNumberTree(500));
            Assert.Equal(value, (PdfInteger)await tree.Search(key));
        }

        [Theory]
        [InlineData(-10)]
        [InlineData(0)]
        [InlineData(101.5)]
        [InlineData(501)]
        [InlineData(1000)]
        public Task SearchFails(double key)
        {
            var tree = new PdfTree<PdfNumber>(CreateNumberTree(500));
            return Assert.ThrowsAsync<PdfParseException>(()=> tree.Search(key).AsTask());
        }

        [Fact]
        public async Task IteratorTest()
        {
            Assert.Equal(Enumerable.Range(1,500).Select(i=>10*i).ToList(), await 
                new PdfTree<PdfNumber>(CreateNumberTree(500))
                    .Select(i=>(int)((PdfNumber)i).IntValue)
                    .ToListAsync() );
            
        }
    }
}