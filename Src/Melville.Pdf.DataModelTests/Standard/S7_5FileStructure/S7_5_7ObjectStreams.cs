using System;
using System.IO;
using System.IO.Pipelines;
using System.Linq;
using System.Threading.Tasks;
using Melville.FileSystem;
using Melville.Parsing.Streams;
using Melville.Pdf.DataModelTests.ParsingTestUtils;
using Melville.Pdf.LowLevel;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Model.Objects2;
using Melville.Pdf.LowLevel.Model.Primitives;
using Melville.Pdf.LowLevel.Parsing.ObjectParsers;
using Melville.Pdf.LowLevel.Parsing.ObjectParsers2;
using Melville.Pdf.LowLevel.Parsing.ObjectParsers2.IndirectValues;
using Melville.Pdf.LowLevel.Parsing.ParserContext;
using Melville.Pdf.LowLevel.Writers;
using Melville.Pdf.LowLevel.Writers.Builder;
using Melville.Pdf.LowLevel.Writers.DocumentWriters;
using Melville.Pdf.ReferenceDocuments.LowLevel.Encryption;
using Xunit;

namespace Melville.Pdf.DataModelTests.Standard.S7_5FileStructure;

public class S7_5_7ObjectStreams
{
    [Theory]
    [InlineData("1 0 2 6 11111\n22222")]
    [InlineData("1 0 2 5 1111122222")]
    [InlineData("1 0 2 7 11111\n222222")]
    public async Task RawLoadObjectStreamAsync(string streamText)
    {
        var os = new ValueDictionaryBuilder()
            .WithItem(KnownNames.TypeTName, KnownNames.ObjStmTName)
            .WithItem(KnownNames.NTName, 2)
            .WithItem(KnownNames.FirstTName, 8)
            .AsStream(streamText);

        var pfo = new ParsingFileOwner(new MemoryStream(), NullPasswordSource.Instance);
        var res = pfo.NewIndirectResolver;
        res.RegisterObjectStreamBlock(1, 10, 0);
        res.RegisterObjectStreamBlock(2, 10, 1);
        res.RegisterDirectObject(10, 0, os, false);

        await AssertIndirectAsync(res, 1, "11111");
        await AssertIndirectAsync(res, 2, "22222");
    }

    private static async Task AssertIndirectAsync(IndirectValueRegistry res, int objectNumber, string expected)
    {
        var o1 = await res.LookupAsync(objectNumber, 0);
        Assert.Equal(expected, o1.ToString());
    }

    [Fact]
    public async Task EmptyObjectStreamAsync()
    {
        //this tests that empty object streams can exist.
        // it also tests the error condition where an object is referenced to an ordinal off the end of the object stream.
        var os = new ValueDictionaryBuilder()
            .WithItem(KnownNames.TypeTName, KnownNames.ObjStmTName)
            .WithItem(KnownNames.NTName, 0)
            .WithItem(KnownNames.FirstTName, 0)
            .AsStream(Array.Empty<byte>());

        var pfo = new ParsingFileOwner(new MemoryStream(), NullPasswordSource.Instance);
        var res = pfo.NewIndirectResolver;
        res.RegisterObjectStreamBlock(1, 10, 0);
        res.RegisterDirectObject(10, 0, os, false);

        await AssertIndirectAsync(res, 1, "null");

    }
    [Theory]
    [InlineData("/Type/ObjStm")]
    [InlineData("/N 2")]
    [InlineData("/First 8")]
    [InlineData("1 0 2 6 (One)\n(Two)")]
    public async Task RenderObjectStreamAsync(string expected)
    {
        Assert.Contains(expected, await DocWithObjectStreamAsync());
    }

    [Fact]
    public async Task MaxObjectInObjStreamAsync()
    {
        Assert.Contains("/Size 22", await DocWithObjectStreamWithHighObjectNumberAsync());
    }

    [Fact]
    public async Task RoundTripAsync()
    {
        var doc = await (await DocWithObjectStreamAsync()).ParseDocumentAsync();
        Assert.Equal("One", (await doc.Objects[(1,0)].LoadValueAsync()).ToString());
            
    }

    [Fact]
    public void CannotPutStreamInObjectStream()
    {
        Assert.False(new ObjectStreamBuilder().TryAddRef(2,new ValueDictionaryBuilder().AsStream("Hello")));
    }
        
    private static async Task<string> DocWithObjectStreamAsync()
    {
        var builder = LowLevelDocumentBuilderFactory.New();
        using (builder.ObjectStreamContext(new ValueDictionaryBuilder()))
        {
            builder.Add(PdfDirectValue.CreateString("One"u8));
            builder.Add(PdfDirectValue.CreateString("Two"u8));
        }
        var fileAsString = await DocCreatorToStringAsync(builder);
        return fileAsString;
    }
    private static async Task<string> DocWithObjectStreamWithHighObjectNumberAsync()
    {
        var builder = LowLevelDocumentBuilderFactory.New();
        using (builder.ObjectStreamContext(new ValueDictionaryBuilder()))
        {
            builder.Add(PdfDirectValue.CreateString("One"u8));
            builder.Add(PdfDirectValue.CreateString("Two"u8), 20, 1);
        }
        var fileAsString = await DocCreatorToStringAsync(builder);
        return fileAsString;
    }

    private static async Task<string> DocCreatorToStringAsync(ILowLevelDocumentCreator builder)
    {
        var ms = new MultiBufferStream();
        var writer = new XrefStreamLowLevelDocumentWriter(PipeWriter.Create(ms), builder.CreateDocument());
        await writer.WriteAsync();
        var fileAsString = ms.CreateReader().ReadToArray().ExtendedAsciiString();
        return fileAsString;
    }

    [Fact]
    public async Task ExtractIncludedObjectReferencesAsync()
    {
        var builder = new ObjectStreamBuilder();
        builder.TryAddRef(1,PdfDirectValue.CreateString("One"u8));
        builder.TryAddRef(2,PdfDirectValue.CreateString("Two"u8));
        var str = (await builder.CreateStreamAsync()).Get<PdfValueStream>();

        var output = await str.GetIncludedObjectNumbersAsync();
        Assert.Equal(1, output[0].ObjectNumber);
        Assert.Equal(2, output[1].ObjectNumber);
    }

    [Fact]
    public async Task DoNotEncryptStringsInContentStreamsAsync()
    {
        var ms = new MemoryStream();
        await new EncryptedRefStm().WritePdfAsync(ms);
        var extendedAsciiString = ms.ToArray().ExtendedAsciiString();
        Assert.DoesNotContain("String In", extendedAsciiString);
        ms.Seek(0, SeekOrigin.Begin);
        
        // string is inside an encrypted stream and so no plaintext
        var doc = await 
            new PdfLowLevelReader(new ConstantPasswordSource(PasswordType.User, "User")).ReadFromAsync(ms);
        var embeddedStream = "String in Stream Context.";
        Assert.Equal(embeddedStream, (await doc.Objects[(2,0)].LoadValueAsync()).ToString());
        
        // string is not encoded inside stream
        var objSter = (await doc.Objects[(3, 0)].LoadValueAsync()).Get<PdfValueStream>();
        var strText = await new StreamReader(await objSter.StreamContentAsync()).ReadToEndAsync();
        Assert.Contains(strText, strText);
    }
}