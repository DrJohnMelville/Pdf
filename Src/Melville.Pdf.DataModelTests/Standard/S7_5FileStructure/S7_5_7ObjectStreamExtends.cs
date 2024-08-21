using System;
using System.IO;
using System.Threading.Tasks;
using Melville.Parsing.MultiplexSources;
using Melville.Parsing.ObjectRentals;
using Melville.Parsing.Streams;
using Melville.Pdf.LowLevel;
using Melville.Pdf.LowLevel.Model.Conventions;
using Xunit;

namespace Melville.Pdf.DataModelTests.Standard.S7_5FileStructure;

public readonly struct MultiBufferWriter
{
    private readonly IWritableMultiplexSource buffer = WritableBuffer.Create();

    public MultiBufferWriter() { }

    public long Write(string s)
    {
        using var writer = buffer.WritingStream();
        writer.Seek(0, SeekOrigin.End);
        Span<byte> scratch = stackalloc byte[s.Length];
        ExtendedAsciiEncoding.EncodeToSpan(s, scratch);
        writer.Write(scratch);
        return buffer.Length;
    }

    public Stream CreateReader()
    {
        try
        {
            return buffer.ReadFrom(0);
        }
        finally
        {
            buffer.Dispose();
        }
    }
}

public class S7_5_7ObjectStreamExtends
{
    [Fact]
    public async Task NonextndedStreamTestAsync()
    {
        var mbs = CreateFile("""
        1 0 obj
        <</Type /ObjStm
        /Length 21
        /N 2
        /First 8>>
        stream
        2 0 3 5 1234 (String)
        endstream
        
        """, """
        4 0 obj (Placehoder stream) endobj
 
        """);
        await using var r2 = mbs.CreateReader();
        var reader = await new PdfLowLevelReader().ReadFromAsync(r2);
        Assert.Equal("1234", (await reader.Objects[(2,0)].LoadValueAsync()).ToString());
        Assert.Equal("String", (await reader.Objects[(3,0)].LoadValueAsync()).ToString());
    }
    [Fact]
    public async Task ExtendedStreamTestAsync()
    {
        var mbs = CreateFile("""
        1 0 obj
        <</Type /ObjStm
        /Length 21
        /N 1
        /First 4
        /Extends 4 0 R>>
        stream
        2 0 1234
        endstream
        
        """, """
        4 0 obj <<
        /Type /ObjStm
        /N 1
        /First 4
        /Length 12>>
        stream
        3 0 (String)
        endstream
 
        """);
        await using var r2 = mbs.CreateReader();
        var reader = await new PdfLowLevelReader().ReadFromAsync(r2);
        Assert.Equal("1234", (await reader.Objects[(2,0)].LoadValueAsync()).ToString());

        Assert.Equal("String", (await reader.Objects[(3,0)].LoadValueAsync()).ToString());
    }
    [Fact]
    public async Task OverridePriorTestAsync()
    {
        var mbs = CreateFile("""
        1 0 obj
        <</Type /ObjStm
        /Length 21
        /N 2
        /First 8
        /Extends 4 0 R>>
        stream
        2 0 3 5 1234 (NewStr)
        endstream
        
        """, """
        4 0 obj <<
        /Type /ObjStm
        /N 1
        /First 4
        /Length 12>>
        stream 
        3 0 (String)
        endstream
 
        """);
        await using var r2 = mbs.CreateReader();
        var reader = await new PdfLowLevelReader().ReadFromAsync(r2);
        Assert.Equal("1234", (await reader.Objects[(2,0)].LoadValueAsync()).ToString());
        Assert.Equal("NewStr", (await reader.Objects[(3,0)].LoadValueAsync()).ToString());
    }

    private static MultiBufferWriter CreateFile(string firstStream, string SecondStream)
    {
        var mbs = new MultiBufferWriter();
        var xsPos = mbs.Write("%PDF-2.0\r\n");
        var secondRefPos = mbs.Write(firstStream);
        var trailerPos = mbs.Write(SecondStream);
        mbs.Write($"""
        <<
        /Type /CRef
        /Index [0 5]
        /W [1 2 1]
        /Filter /ASCIIHexDecode
        /Size 5
        /Length 20000
        >>
        stream
        00 0000 FF
        01 {xsPos:X4}  00
        02 0001 00
        02 0001 01
        01 {secondRefPos:X4}  00>
        endstream
        startxref
        { trailerPos} 
        %%EOF
        """ );
        return mbs;
    }
}