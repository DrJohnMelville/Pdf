using System;
using System.IO;
using System.IO.Pipelines;
using System.Threading.Tasks;
using Melville.FileSystem;
using Melville.Parsing.MultiplexSources;
using Melville.Parsing.ObjectRentals;
using Melville.Parsing.Streams;
using Melville.Pdf.DataModelTests.ParsingTestUtils;
using Melville.Pdf.LowLevel;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Model.Objects.StreamParts;
using Melville.Pdf.LowLevel.Parsing.ObjectParsers.IndirectValues;
using Melville.Pdf.LowLevel.Parsing.ParserContext;
using Melville.Pdf.LowLevel.Writers.Builder;
using Melville.Pdf.LowLevel.Writers.DocumentWriters;
using Melville.Pdf.ReferenceDocuments.LowLevel.Encryption;
using Moq;
using Xunit;

namespace Melville.Pdf.DataModelTests.Standard.S7_5FileStructure;

public class S7_5_7ObjectStreams: IDisposable
{
    private IDisposable ctx = RentalPolicyChecker.RentalScope();
    public void Dispose() => ctx.Dispose();

    [Theory]
    [InlineData("1 0 2 6 11111\n22222")]
    [InlineData("1 0 2 5 1111122222")]
    [InlineData("1 0 2 7 11111\n222222")]
    public async Task RawLoadObjectStreamAsync(string streamText)
    {
        var os = new DictionaryBuilder()
            .WithItem(KnownNames.Type, KnownNames.ObjStm)
            .WithItem(KnownNames.N, 2)
            .WithItem(KnownNames.First, 8)
            .AsStream(streamText);

        using var pfo = new ParsingFileOwner(MultiplexSourceFactory.Create(Array.Empty<byte>()), NullPasswordSource.Instance);
        var res = pfo.NewIndirectResolver;
        res.RegisterObjectStreamBlock(1, 10, 0);
        res.RegisterObjectStreamBlock(2, 10, 1);
        res.RegisterDirectObject(10, 0, os, false);

        await AssertIndirectAsync(res, 1, "11111");
        await AssertIndirectAsync(res, 2, "22222");
    }

    private static async Task AssertIndirectAsync(IndirectObjectRegistry res, int objectNumber, string expected)
    {
        var o1 = await res.LookupAsync(objectNumber, 0);
        Assert.Equal(expected, o1.ToString());
    }

    [Fact]
    public async Task EmptyObjectStreamAsync()
    {
        //this tests that empty object streams can exist.
        // it also tests the error condition where an object is referenced to an ordinal off the end of the object stream.
        var os = new DictionaryBuilder()
            .WithItem(KnownNames.Type, KnownNames.ObjStm)
            .WithItem(KnownNames.N, 0)
            .WithItem(KnownNames.First, 0)
            .AsStream(Array.Empty<byte>());

        var pfo = new ParsingFileOwner(MultiplexSourceFactory.Create(Array.Empty<byte>()), NullPasswordSource.Instance);
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
        Assert.False(new ObjectStreamBuilder().TryAddRef(2,new DictionaryBuilder().AsStream("Hello")));
    }
        
    private static async Task<string> DocWithObjectStreamAsync()
    {
        var builder = LowLevelDocumentBuilderFactory.New();
        using (builder.ObjectStreamContext(new DictionaryBuilder()))
        {
            builder.Add(PdfDirectObject.CreateString("One"u8));
            builder.Add(PdfDirectObject.CreateString("Two"u8));
        }
        var fileAsString = await DocCreatorToStringAsync(builder);
        return fileAsString;
    }
    private static async Task<string> DocWithObjectStreamWithHighObjectNumberAsync()
    {
        var builder = LowLevelDocumentBuilderFactory.New();
        using (builder.ObjectStreamContext(new DictionaryBuilder()))
        {
            builder.Add(PdfDirectObject.CreateString("One"u8));
            builder.Add(PdfDirectObject.CreateString("Two"u8), 20, 1);
        }
        var fileAsString = await DocCreatorToStringAsync(builder);
        return fileAsString;
    }

    private static async Task<string> DocCreatorToStringAsync(ILowLevelDocumentCreator builder)
    {
        var ms = new MemoryStream();
        var writer = new XrefStreamLowLevelDocumentWriter(PipeWriter.Create(ms), builder.CreateDocument());
        await writer.WriteAsync();
        var fileAsString = ms.GetBuffer().AsSpan(0,(int)ms.Length).ExtendedAsciiString();
        return fileAsString;
    }

    [Fact]
    public async Task ExtractIncludedObjectReferencesAsync()
    {
        var builder = new ObjectStreamBuilder();
        builder.TryAddRef(1,PdfDirectObject.CreateString("One"u8));
        builder.TryAddRef(2,PdfDirectObject.CreateString("Two"u8));
        var str = (await builder.CreateStreamAsync()).Get<PdfStream>();

        var target = new Mock<IInternalObjectTarget>();
        await str.ReportIncludedObjectsAsync(new(target.Object, 15));

        target.Verify(i=>i.DeclareObjectStreamObjectAsync(1, 15, 0, 8));
        target.Verify(i=>i.DeclareObjectStreamObjectAsync(2, 15, 1, 14));
        target.VerifyNoOtherCalls();
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
        var objSter = (await doc.Objects[(3, 0)].LoadValueAsync()).Get<PdfStream>();
        var strText = await new StreamReader(await objSter.StreamContentAsync()).ReadToEndAsync();
        Assert.Contains(strText, strText);
    }
}