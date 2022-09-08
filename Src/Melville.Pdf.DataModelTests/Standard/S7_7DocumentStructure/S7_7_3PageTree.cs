using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Castle.DynamicProxy.Generators.Emitters.SimpleAST;
using Melville.Pdf.DataModelTests.ParsingTestUtils;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.Model.Creators;
using Melville.Pdf.Model.Documents;
using Xunit;

namespace Melville.Pdf.DataModelTests.Standard.S7_7DocumentStructure;

public class S7_7_3PageTree
{
    [Fact]
    public async Task CreateThreePages()
    {
        var doc = CreateThreePageSimpleDocument();

        var pagesAsync = await doc.PagesAsync();
        Assert.Equal(3, await pagesAsync.CountAsync());
        Assert.Equal(3, await ((IAsyncEnumerable<PdfPage>)(await doc.PagesAsync())).CountAsync());
    }

    private static PdfDocument CreateThreePageSimpleDocument()
    {
        var creator = new PdfDocumentCreator();
        creator.Pages.CreatePage();
        creator.Pages.CreatePage();
        creator.Pages.CreatePage();

        var doc = new PdfDocument(creator.CreateDocument());
        return doc;
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    public async Task GetSimplePageFromIndex(int position)
    {
        var doc = CreateThreePageSimpleDocument();
        var pagesTree = await doc.PagesAsync();
        Assert.Equal(await pagesTree.ElementAtAsync(position),
            await pagesTree.GetPageAsync(position+1));
        
    }
    private static PdfDocument CreateFourPageComplexDocument()
    {
        var creator = new PdfDocumentCreator();
        creator.Pages.CreatePage();
        var node = creator.Pages.CreateNode();
        node.CreatePage();
        node.CreatePage();
        creator.Pages.CreatePage();

        var doc = new PdfDocument(creator.CreateDocument());
        return doc;
    }
    [Fact]
    public async Task CreateComplexThreePages()
    {
        var doc = CreateFourPageComplexDocument();

        Assert.Equal(4, await (await doc.PagesAsync()).CountAsync());
        Assert.Equal(4, await ((IAsyncEnumerable<PdfPage>)(await doc.PagesAsync())).CountAsync());
    }
    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    public async Task GetComplexPageFromIndex(int position)
    {
        var doc = CreateFourPageComplexDocument();
        var pagesTree = await doc.PagesAsync();
        await TestEnumeratorAndAccessor(position, pagesTree);
        
    }

    private static async Task TestEnumeratorAndAccessor(int position, PageTree pagesTree)
    {
        var btIndex = await pagesTree.ElementAtAsync(position);
        var byPageNumber = await pagesTree.GetPageAsync(position+1);
        Assert.Equal(btIndex,
            byPageNumber);
    }

    [Fact]
    public async Task DefaultTreeSizeTest()
    {
        var doc = FifteenPageDocumentTree();
        var p0 = await (await doc.PagesAsync()).GetPageAsync(0);
        Assert.Equal(KnownNames.Page, await p0.LowLevel.GetAsync<PdfName>(KnownNames.Type));
        var p1 = await p0.GetParentAsync();
        Assert.Equal(KnownNames.Pages, 
            await ((HasRenderableContentStream)p1!).LowLevel.GetAsync<PdfName>(KnownNames.Type));
        Assert.True(p1 != null);
        var p2 = await p1!.GetParentAsync();
        Assert.True(p2 != null);
        Assert.Equal(KnownNames.Pages, 
            await ((HasRenderableContentStream)p2!).LowLevel.GetAsync<PdfName>(KnownNames.Type));
        var p3 = await p2.GetParentAsync();
        Assert.True(p3!= null);
        Assert.Equal(KnownNames.Pages, 
            await ((HasRenderableContentStream)p3!).LowLevel.GetAsync<PdfName>(KnownNames.Type));
        var p4 = await p3.GetParentAsync();
        Assert.False(p4 != null);
    }

    private static PdfDocument FifteenPageDocumentTree()
    {
        var creator = new PdfDocumentCreator() { MaxPageTreeNodeSize = 3 };
        for (int i = 0; i < 15; i++)
        {
            creator.Pages.CreatePage();
        }

        var doc = new PdfDocument(creator.CreateDocument());
        return doc;
    }

    [Fact]
    public async Task TestMultiLevelTreeaccess()
    {
        var doc = FifteenPageDocumentTree();
        for (int i = 0; i < 15; i++)
        {
            await TestEnumeratorAndAccessor(i, await doc.PagesAsync());
        }
    }
}