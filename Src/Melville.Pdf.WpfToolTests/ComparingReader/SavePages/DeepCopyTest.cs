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
    private readonly Mock<ILowLevelDocumentBuilder> creator = new();
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

    [Fact] public Task CopyInt() => PassthroughCopy(1);
    [Fact] public Task CopyReal() => PassthroughCopy(1.5);
    [Fact] public Task CopyString() => PassthroughCopy(PdfString.CreateAscii("Hello World"));
    [Fact] public Task CopyName() => PassthroughCopy(KnownNames.Length);
    [Fact] public Task CopyBool() => PassthroughCopy(PdfBoolean.True);
    [Fact] public Task CopyNull() => PassthroughCopy(PdfTokenValues.Null);

    private async Task PassthroughCopy(PdfObject item)
    {
        var copy = await sut.Clone(item);
        Assert.Same(item, copy);
    }

    [Fact]
    public async Task CopyArray()
    {
        var datum = new PdfArray(1, 
            new PdfArray(2),
            new PdfIndirectObject(1,1,PdfBoolean.False));
        var clone = await sut.Clone(datum);
        await DeepAssertSame(datum, clone);
    }
    [Fact]
    public async Task CopyDictionary()
    {
        var datum = new DictionaryBuilder()
            .WithItem(KnownNames.A85, 23)
            .WithItem(KnownNames.AllOn, new DictionaryBuilder()
                .WithItem(KnownNames.All, KnownNames.W)
                .AsDictionary())
            .AsDictionary();
        var clone = await sut.Clone(datum);
        await DeepAssertSame(datum, clone);
    }
    [Fact]
    public async Task Stream()
    {
        var datum = new DictionaryBuilder()
            .WithItem(KnownNames.A85, 23)
            .WithItem(KnownNames.AllOn, new DictionaryBuilder()
                .WithItem(KnownNames.All, KnownNames.W)
                .AsDictionary())
            .AsStream("Hello World", StreamFormat.DiskRepresentation);
        var clone = await sut.Clone(datum);
        await DeepAssertSame(datum, clone);
    }

    private async ValueTask DeepAssertSame(PdfObject a, PdfObject b)
    {
        switch (a)
        {
            case PdfIndirectObject: await AssertSameIndirectobject(a, b); break;
            case PdfStream ps: await AssertSameStream(ps, (PdfStream)b); break;
            case PdfDictionary pa: await AssertSameDictionary(pa, (PdfDictionary)b); break;
            case PdfArray pa: await AssertSameArray(pa, (PdfArray)b); break;
            default: Assert.Same(a,b); break;
        }
    }

    private async Task AssertSameStream(PdfStream a, PdfStream b)
    {
        await AssertSameDictionary(a, b);
        var aVal = await AsArray(await a.StreamContentAsync());
        var bVal = await AsArray(await b.StreamContentAsync());
        Assert.Equal(aVal, bVal);
    }

    private async ValueTask<byte[]> AsArray(Stream str)
    {
        var ret = new MemoryStream();
        await str.CopyToAsync(ret);
        return ret.ToArray();

    }

    private async Task AssertSameIndirectobject(PdfObject a, PdfObject b)
    {
        Assert.NotSame(a, b);
        Assert.IsType<PromisedIndirectObject>(b);
        Assert.IsType<PdfIndirectObject>(a);
        await DeepAssertSame(await a.DirectValueAsync(), await b.DirectValueAsync());
    }

    private async Task AssertSameDictionary(PdfDictionary a, PdfDictionary b)
    {
        Assert.Equal(a.Count, b.Count);
        foreach (var (ai,bi) in a.RawItems.Zip(b.RawItems, (i,j)=>(i,j)))
        {
            await DeepAssertSame(ai.Key, bi.Key);
            await DeepAssertSame(ai.Value, bi.Value);
        }
    }

    private async ValueTask AssertSameArray(PdfArray a, PdfArray b)
    {
        Assert.Equal(a.Count,b.Count);
        for (int i = 0; i < a.Count; i++)
        {
            await DeepAssertSame(a.RawItems[i], b.RawItems[i]);
        }
    }
}