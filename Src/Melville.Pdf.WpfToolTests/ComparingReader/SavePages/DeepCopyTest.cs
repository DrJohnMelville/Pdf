using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Melville.Pdf.ComparingReader.SavePagesImpl;
using Melville.Pdf.DataModelTests.ParsingTestUtils;
using Melville.Pdf.LowLevel.Filters.FilterProcessing;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Model.Objects2;
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
        creator.Setup(i => i.Add(It.IsAny<PdfDirectValue>())).Returns((PdfDirectValue i) => i switch
        {
            _ => new PdfIndirectValue(default, 5, 6)
        });
//        creator.Setup(i => i.AddPromisedObject()).Returns(() => new PromisedIndirectObject(3, 4));
        sut = new DeepCopy(creator.Object);
    }

    [Fact] public Task CopyIntAsync() => PassthroughCopyAsync(1);
    [Fact] public Task CopyRealAsync() => PassthroughCopyAsync(1.5);
    [Fact] public Task CopyStringAsync() => PassthroughCopyAsync("Hello World");
    [Fact] public Task CopyNameAsync() => PassthroughCopyAsync(KnownNames.LengthTName);
    [Fact] public Task CopyBoolAsync() => PassthroughCopyAsync(true);
    [Fact] public Task CopyNullAsync() => PassthroughCopyAsync(PdfDirectValue.CreateNull());

    private async Task PassthroughCopyAsync(PdfDirectValue item)
    {
        var copy = await sut.CloneAsync(item);
        Assert.Equal(item, copy);
    }

    [Fact]
    public async Task CopyArrayAsync()
    {
        var datum = new PdfValueArray(1, 
            new PdfValueArray(2),
            new PdfIndirectValue(Mock.Of<IIndirectValueSource>(), 1, 1));
        var clone = await sut.CloneAsync(datum);
        await DeepAssertSameAsync(datum, clone);
    }
    [Fact]
    public async Task CopyDictionaryAsync()
    {
        var datum = new ValueDictionaryBuilder()
            .WithItem(KnownNames.A85TName, 23)
            .WithItem(KnownNames.AllOnTName, new ValueDictionaryBuilder()
                .WithItem(KnownNames.AllTName, KnownNames.WTName)
                .AsDictionary())
            .AsDictionary();
        var clone = await sut.CloneAsync(datum);
        await DeepAssertSameAsync(datum, clone);
    }
    [Fact]
    public async Task StreamAsync()
    {
        var datum = new ValueDictionaryBuilder()
            .WithItem(KnownNames.A85TName, 23)
            .WithItem(KnownNames.AllOnTName, new ValueDictionaryBuilder()
                .WithItem(KnownNames.AllTName, KnownNames.WTName)
                .AsDictionary())
            .AsStream("Hello World", StreamFormat.DiskRepresentation);
        var clone = await sut.CloneAsync(datum);
        await DeepAssertSameAsync(datum, clone);
    }

    private async ValueTask DeepAssertSameAsync(PdfIndirectValue a, PdfIndirectValue b)
    {
        switch (a)
        {
            case var x when x.IsEmbeddedDirectValue(): await AssertSameIndirectobjectAsync(a, b); break;
            case var x when Force(a,b, out PdfValueStream? aval, out var bval): await AssertSameStreamAsync(aval, bval); break;
            case var x when Force(a,b, out PdfValueDictionary? aval, out var bval): await AssertSameDictionaryAsync(aval, bval); break;
            case var x when Force(a,b, out PdfValueArray? aval, out var bval): await AssertSameArrayAsync(aval, bval); break;
            default: Assert.Same(a,b); break;
        }
    }

    private static bool Force<T>(PdfIndirectValue a, PdfIndirectValue  b, out T aval, out T bval)
    {
        aval = bval = default;
        return a.TryGetEmbeddedDirectValue(out var y) && y.TryGet(out aval) &&
          b.TryGetEmbeddedDirectValue(out var y2) && y2.TryGet(out bval);
    }

    private async Task AssertSameStreamAsync(PdfValueStream a, PdfValueStream b)
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

    private async Task AssertSameIndirectobjectAsync(PdfIndirectValue a, PdfIndirectValue b)
    {
        Assert.Equal(a, b);
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

    private async ValueTask AssertSameArrayAsync(PdfValueArray a, PdfValueArray b)
    {
        Assert.Equal(a.Count,b.Count);
        for (int i = 0; i < a.Count; i++)
        {
            await DeepAssertSameAsync(a.RawItems[i], b.RawItems[i]);
        }
    }
}