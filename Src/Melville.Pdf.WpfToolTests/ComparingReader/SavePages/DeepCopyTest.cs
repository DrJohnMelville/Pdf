using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Melville.Pdf.ComparingReader.SavePagesImpl;
using Melville.Pdf.DataModelTests.ParsingTestUtils;
using Melville.Pdf.LowLevel.Filters.FilterProcessing;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Model.Primitives;
using Melville.Pdf.LowLevel.Writers;
using Melville.Pdf.LowLevel.Writers.Builder;
using Melville.Pdf.Model.Creators;
using Moq;
using Xunit;
using DeepCopy = Melville.Pdf.LowLevel.Writers.PageExtraction.DeepCopy;

namespace Melville.Pdf.WpfToolTests.ComparingReader.SavePages;

public class DeepCopyTest
{
    private readonly Mock<IPdfObjectCreatorRegistry> creator = new();
    private readonly DeepCopy sut;

    public DeepCopyTest()
    {
        creator.Setup(i => i.Add(It.IsAny<PdfObject>())).Returns((PdfObject i) => i switch
        {
            null => throw new ArgumentException(),
            PdfIndirectObject pio => pio,
            _ => new PdfIndirectObject(5, 6, i)
        });
        creator.Setup(i => i.AddPromisedObject()).Returns(() => new PromisedIndirectObject(3, 4));
        sut = new DeepCopy(creator.Object);
    }

    [Fact] public Task CopyIntAsync() => PassthroughCopyAsync(1);
    [Fact] public Task CopyRealAsync() => PassthroughCopyAsync(1.5);
    [Fact] public Task CopyStringAsync() => PassthroughCopyAsync(PdfString.CreateAscii("Hello World"));
    [Fact] public Task CopyNameAsync() => PassthroughCopyAsync(KnownNames.Length);
    [Fact] public Task CopyBoolAsync() => PassthroughCopyAsync(PdfBoolean.True);
    [Fact] public Task CopyNullAsync() => PassthroughCopyAsync(PdfTokenValues.Null);

    private async Task PassthroughCopyAsync(PdfObject item)
    {
        var copy = await sut.CloneAsync(item);
        Assert.Same(item, copy);
    }

    [Fact]
    public async Task CopyArrayAsync()
    {
        var datum = new PdfArray(1, 
            new PdfArray(2),
            new PdfIndirectObject(1,1,PdfBoolean.False));
        var clone = await sut.CloneAsync(datum);
        await DeepAssertSameAsync(datum, clone);
    }
    [Fact]
    public async Task CopyDictionaryAsync()
    {
        var datum = new DictionaryBuilder()
            .WithItem(KnownNames.A85, 23)
            .WithItem(KnownNames.AllOn, new DictionaryBuilder()
                .WithItem(KnownNames.All, KnownNames.W)
                .AsDictionary())
            .AsDictionary();
        var clone = await sut.CloneAsync(datum);
        await DeepAssertSameAsync(datum, clone);
    }
    [Fact]
    public async Task StreamAsync()
    {
        var datum = new DictionaryBuilder()
            .WithItem(KnownNames.A85, 23)
            .WithItem(KnownNames.AllOn, new DictionaryBuilder()
                .WithItem(KnownNames.All, KnownNames.W)
                .AsDictionary())
            .AsStream("Hello World", StreamFormat.DiskRepresentation);
        var clone = await sut.CloneAsync(datum);
        await DeepAssertSameAsync(datum, clone);
    }

    private async ValueTask DeepAssertSameAsync(PdfObject a, PdfObject b)
    {
        switch (a)
        {
            case PdfIndirectObject: await AssertSameIndirectobjectAsync(a, b); break;
            case PdfStream ps: await AssertSameStreamAsync(ps, (PdfStream)b); break;
            case PdfDictionary pa: await AssertSameDictionaryAsync(pa, (PdfDictionary)b); break;
            case PdfArray pa: await AssertSameArrayAsync(pa, (PdfArray)b); break;
            default: Assert.Same(a,b); break;
        }
    }

    private async Task AssertSameStreamAsync(PdfStream a, PdfStream b)
    {
        await AssertSameDictionaryAsync(a, b);
        var aVal = await AsArrayAsync(await a.StreamContentAsync());
        var bVal = await AsArrayAsync(await b.StreamContentAsync());
        Assert.Equal(aVal, bVal);
    }

    private async ValueTask<byte[]> AsArrayAsync(Stream str)
    {
        var ret = new MemoryStream();
        await str.CopyToAsync(ret);
        return ret.ToArray();

    }

    private async Task AssertSameIndirectobjectAsync(PdfObject a, PdfObject b)
    {
        Assert.NotSame(a, b);
        Assert.IsType<PromisedIndirectObject>(b);
        Assert.IsType<PdfIndirectObject>(a);
        await DeepAssertSameAsync(await a.DirectValueAsync(), await b.DirectValueAsync());
    }

    private async Task AssertSameDictionaryAsync(PdfDictionary a, PdfDictionary b)
    {
        Assert.Equal(a.Count, b.Count);
        foreach (var (ai,bi) in a.RawItems.Zip(b.RawItems, (i,j)=>(i,j)))
        {
            await DeepAssertSameAsync(ai.Key, bi.Key);
            await DeepAssertSameAsync(ai.Value, bi.Value);
        }
    }

    private async ValueTask AssertSameArrayAsync(PdfArray a, PdfArray b)
    {
        Assert.Equal(a.Count,b.Count);
        for (int i = 0; i < a.Count; i++)
        {
            await DeepAssertSameAsync(a.RawItems[i], b.RawItems[i]);
        }
    }
}