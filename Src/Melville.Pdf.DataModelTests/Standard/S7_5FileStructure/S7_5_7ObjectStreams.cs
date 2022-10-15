using System;
using System.IO;
using System.IO.Pipelines;
using System.Linq;
using System.Threading.Tasks;
using Melville.FileSystem;
using Melville.Parsing.Streams;
using Melville.Pdf.DataModelTests.ParsingTestUtils;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Model.Primitives;
using Melville.Pdf.LowLevel.Parsing.ObjectParsers;
using Melville.Pdf.LowLevel.Parsing.ParserContext;
using Melville.Pdf.LowLevel.Writers;
using Melville.Pdf.LowLevel.Writers.Builder;
using Melville.Pdf.LowLevel.Writers.DocumentWriters;
using Xunit;

namespace Melville.Pdf.DataModelTests.Standard.S7_5FileStructure;

public class S7_5_7ObjectStreams
{
    
    [Theory]
    [InlineData("1 0 2 6 11111\n22222")]
    [InlineData("1 0 2 5 1111122222")]
    [InlineData("1 0 2 7 11111\n222222")]
    public async Task RawLoadObjectStream(string streamText)
    {
        var os = new DictionaryBuilder()
            .WithItem(KnownNames.Type, KnownNames.ObjStm)
            .WithItem(KnownNames.N, 2)
            .WithItem(KnownNames.First, 8)
            .AsStream(streamText);

        var res = new IndirectObjectResolver();
        var pfo = new ParsingFileOwner(new MemoryStream(), NullPasswordSource.Instance, res);
        res.AddLocationHint(new ObjectStreamIndirectObject(1, 0, pfo, 10));
        res.AddLocationHint(new ObjectStreamIndirectObject(2, 0, pfo, 10));
        res.AddLocationHint(new PdfIndirectObject(10, 0, os));

        await AssertIndirect(res, 1, "11111");
        await AssertIndirect(res, 2, "22222");
    }

    [Fact]
    public async Task EmptyObjectStream()
    {
        var os = new DictionaryBuilder()
            .WithItem(KnownNames.Type, KnownNames.ObjStm)
            .WithItem(KnownNames.N, 0)
            .WithItem(KnownNames.First, 0)
            .AsStream(Array.Empty<byte>());

        var res = new IndirectObjectResolver();
        var pfo = new ParsingFileOwner(new MemoryStream(), NullPasswordSource.Instance, res);

        await ObjectStreamIndirectObject.LoadObjectStream(pfo, os);

    }

    private static async Task AssertIndirect(IndirectObjectResolver res, int objectNumber, string expected)
    {
        var o1 = await res.FindIndirect(objectNumber, 0).DirectValueAsync();
        Assert.Equal(expected, o1.ToString());
    }

    [Theory]
    [InlineData("/Type/ObjStm")]
    [InlineData("/N 2")]
    [InlineData("/First 8")]
    [InlineData("1 0 2 6 (One)\n(Two)")]
    public async Task RenderObjectStream(string expected)
    {
        Assert.Contains(expected, await DocWithObjectStream());
    }

    [Fact]
    public async Task MaxObjectInObjStream()
    {
        Assert.Contains("/Length 21", await DocWithObjectStreamWithHighObjectNumber());
    }

    [Fact]
    public async Task RoundTrip()
    {
        var doc = await (await DocWithObjectStream()).ParseDocumentAsync();
        Assert.Equal("One", (await doc.Objects[(1,0)].DirectValueAsync()).ToString());
            
    }

    [Fact]
    public void CannotPutStreamInObjectStream()
    {
        Assert.False(new ObjectStreamBuilder().TryAddRef(
            new PdfIndirectObject(2,0,new DictionaryBuilder().AsStream("Hello"))));
    }
    [Fact]
    public void CannotPutNonZeroGenerationStream()
    {
        Assert.False(new ObjectStreamBuilder().TryAddRef(new PdfIndirectObject(12,1, KnownNames.All)));
    }
        
    private static async Task<string> DocWithObjectStream()
    {
        var builder = new LowLevelDocumentCreator();
        using (builder.ObjectStreamContext(new DictionaryBuilder()))
        {
            builder.Add(PdfString.CreateAscii("One"));
            builder.Add(PdfString.CreateAscii("Two"));
        }
        var fileAsString = await DocCreatorToString(builder);
        return fileAsString;
    }
    private static async Task<string> DocWithObjectStreamWithHighObjectNumber()
    {
        var builder = new LowLevelDocumentCreator();
        using (builder.ObjectStreamContext(new DictionaryBuilder()))
        {
            builder.Add(PdfString.CreateAscii("One"));
            builder.Add(new PdfIndirectObject(20, 0, PdfString.CreateAscii("Two")));
        }
        var fileAsString = await DocCreatorToString(builder);
        return fileAsString;
    }

    private static async Task<string> DocCreatorToString(LowLevelDocumentCreator builder)
    {
        var ms = new MultiBufferStream();
        var writer = new LowLevelDocumentWriter(PipeWriter.Create(ms), builder.CreateDocument());
        await writer.WriteWithReferenceStream();
        var fileAsString = ms.CreateReader().ReadToArray().ExtendedAsciiString();
        return fileAsString;
    }

    [Fact]
    public async Task ExtractIncludedObjectReferences()
    {
        var builder = new ObjectStreamBuilder();
        builder.TryAddRef(new PdfIndirectObject(1,0,PdfString.CreateAscii("One")));
        builder.TryAddRef(new PdfIndirectObject(2,0,PdfString.CreateAscii("Two")));
        var str = (PdfStream)await builder.CreateStream(new DictionaryBuilder());

        var output = await str.GetIncludedObjectNumbersAsync();
        Assert.Equal(1, output[0].ObjectNumber);
        Assert.Equal(2, output[1].ObjectNumber);
    }
}