using System.Linq;
using System.Threading.Tasks;
using Melville.Pdf.DataModelTests.ParsingTestUtils;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Model.Objects2;
using Melville.Pdf.LowLevel.Model.Primitives;
using Melville.Pdf.LowLevel.Model.Wrappers;
using Melville.Pdf.LowLevel.Model.Wrappers.Trees;
using Melville.Pdf.LowLevel.Parsing.ParserContext;
using Melville.Pdf.LowLevel.Writers.Builder;
using Xunit;

namespace Melville.Pdf.DataModelTests.Standard.S7_9CommonDataStructures;

public class S7_9Trees
{
    private readonly ILowLevelDocumentCreator builder = LowLevelDocumentBuilderFactory.New();

    private PdfValueDictionary CreateNumberTree(int count)
    {
        return builder.CreateNumberTree(10,
            Enumerable.Range(1, count)
                .Reverse()
                .Select(i => ((PdfDirectValue)i, (PdfIndirectValue)(i*10))));
    }

    [Fact]
    public async Task CreateTrivialNumberTreeAsync()
    {
        var result = CreateNumberTree(5);
        var array = await result.GetAsync<PdfValueArray>(KnownNames.NumsTName);
        for (int i = 0; i < 5; i++)
        {
            Assert.Equal(1+i, (await array[2*i]).Get<int>());
            Assert.Equal(10*(1+i), (await array[2*i+1]).Get<int>());
        }
    }

    [Fact]
    public async Task TwoLevelTreeAsync()
    {
        var result = CreateNumberTree(50);
        var array = await result.GetAsync<PdfValueArray>(KnownNames.KidsTName);
        builder.Add(result);
        Assert.Equal(5, array.Count);
        var secondNode = await array.GetAsync<PdfValueDictionary>(1);
        Assert.Equal(11, (await (await secondNode.GetAsync<PdfValueArray>(KnownNames.NumsTName))[0]).Get<int>());
        Assert.Equal(110, (await (await secondNode.GetAsync<PdfValueArray>(KnownNames.NumsTName))[1]).Get<int>());
        Assert.Equal(20,  (await (await secondNode.GetAsync<PdfValueArray>(KnownNames.NumsTName))[18]).Get<int>());
        Assert.Equal(200, (await (await secondNode.GetAsync<PdfValueArray>(KnownNames.NumsTName))[19]).Get<int>());
        Assert.Equal(11,  ( await (await secondNode.GetAsync<PdfValueArray>(KnownNames.LimitsTName))[0]).Get<int>());
        Assert.Equal(20,  ( await (await secondNode.GetAsync<PdfValueArray>(KnownNames.LimitsTName))[1]).Get<int>());
        var docAsString = await builder.AsStringAsync();
        Assert.Contains("1 0 obj <</Nums[1 10", docAsString);
        Assert.Contains("6 0 obj <</Kids[1 0 R 2 0 R 3 0 R 4 0 R 5 0 R]>> endobj", docAsString);
    }
    [Fact]
    public async Task ThreeLevelTreeAsync()
    {
        var result = CreateNumberTree(500);
        var array = await result.GetAsync<PdfValueArray>(KnownNames.KidsTName);
        builder.Add(result);
        Assert.Equal(5, array.Count);
        var secondNode = await array.GetAsync<PdfValueDictionary>(1);
        Assert.Equal(101, ( await (await secondNode.GetAsync<PdfValueArray>(KnownNames.LimitsTName))[0]).Get<int>());
        Assert.Equal(200, ( await (await secondNode.GetAsync<PdfValueArray>(KnownNames.LimitsTName))[1]).Get<int>());
        var thirdNode = await (await secondNode.GetAsync<PdfValueArray>(KnownNames.KidsTName)).GetAsync<PdfValueDictionary>(1);
        Assert.Equal(111, (await (await thirdNode.GetAsync<PdfValueArray>(KnownNames.NumsTName))[0]).Get<int>());
        Assert.Equal(1110, (await (await thirdNode.GetAsync<PdfValueArray>(KnownNames.NumsTName))[1]).Get<int>());
        Assert.Equal(120, (await (await thirdNode.GetAsync<PdfValueArray>(KnownNames.NumsTName))[18]).Get<int>());
        Assert.Equal(1200, (await (await thirdNode.GetAsync<PdfValueArray>(KnownNames.NumsTName))[19]).Get<int>());
        Assert.Equal(111, ( await (await thirdNode.GetAsync<PdfValueArray>(KnownNames.LimitsTName))[0]).Get<int>());
        Assert.Equal(120, ( await (await thirdNode.GetAsync<PdfValueArray>(KnownNames.LimitsTName))[1]).Get<int>());
    }

    [Fact]
    public async Task CreateTrivialNameTreeAsync()
    {
        var result = builder.CreateNameTree(10,
            (PdfDirectValue.CreateString("A"u8), PdfDirectValue.CreateString("Alpha"u8)),
            (PdfDirectValue.CreateString("C"u8), PdfDirectValue.CreateString("Charlie"u8)),
            (PdfDirectValue.CreateString("B"u8), PdfDirectValue.CreateString("Bravo"u8))
        );
        var array = await result.GetAsync<PdfValueArray>(KnownNames.NamesTName);
        Assert.Equal("A", (await array[0]).ToString());
        Assert.Equal("Alpha", (await array[1]).ToString());
        Assert.Equal("B", (await array[2]).ToString());
        Assert.Equal("Bravo", (await array[3]).ToString());
        Assert.Equal("C", (await array[4]).ToString());
        Assert.Equal("Charlie", (await array[5]).ToString());
            
    }

    [Theory]
    [InlineData(1,10)]
    [InlineData(213,2130)]
    [InlineData(500,5000)]
    public async Task IndexerTestAsync(int key, int value)
    {
        var tree = new PdfTree(CreateNumberTree(500));
        Assert.Equal(value, (await tree.SearchAsync(key)).Get<int>());
    }

    [Theory]
    [InlineData(-10)]
    [InlineData(0)]
    [InlineData(101.5)]
    [InlineData(501)]
    [InlineData(1000)]
    public Task SearchFailsAsync(double key)
    {
        var tree = new PdfTree(CreateNumberTree(500));
        return Assert.ThrowsAsync<PdfParseException>(()=> tree.SearchAsync(key).AsTask());
    }

    [Fact]
    public async Task IteratorTestAsync()
    {
        Assert.Equal(Enumerable.Range(1,500).Select(i=>10*i).ToList(), await 
            new PdfTree(CreateNumberTree(500))
                .Select(i=>(int)(i).Get<int>())
                .ToListAsync() );
            
    }
}