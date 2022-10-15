using System;
using System.IO;
using System.Threading.Tasks;
using Melville.Parsing.Streams;
using Melville.Pdf.LowLevel;
using Melville.Pdf.LowLevel.Model.Conventions;
using Xunit;

namespace Melville.Pdf.DataModelTests.Standard.S7_5FileStructure;

public readonly struct MultiBufferWriter
{
    private readonly MultiBufferStream buffer = new MultiBufferStream();

    public MultiBufferWriter() { }

    public long Write(string s)
    {
        Span<byte> scratch = stackalloc byte[s.Length];
        ExtendedAsciiEncoding.EncodeToSpan(s, scratch);
        buffer.Write(scratch);
        return buffer.Length;
    }

    public Stream CreateReader() => buffer.CreateReader();
}

public class S7_5_7ObjectStreamExtends
{
    [Fact]
    public async Task NonextndedStreamTest()
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
        var reader = await new PdfLowLevelReader().ReadFromAsync(mbs.CreateReader());
        Assert.Equal("1234", (await reader.Objects[(2,0)].DirectValueAsync()).ToString());
        Assert.Equal("String", (await reader.Objects[(3,0)].DirectValueAsync()).ToString());
    }
    [Fact]
    public async Task ExtendedStreamTest()
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
        var reader = await new PdfLowLevelReader().ReadFromAsync(mbs.CreateReader());
        Assert.Equal("1234", (await reader.Objects[(2,0)].DirectValueAsync()).ToString());
        Assert.Equal("String", (await reader.Objects[(3,0)].DirectValueAsync()).ToString());
    }
    [Fact]
    public async Task OverridePriorTest()
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
        var reader = await new PdfLowLevelReader().ReadFromAsync(mbs.CreateReader());
        Assert.Equal("1234", (await reader.Objects[(2,0)].DirectValueAsync()).ToString());
        Assert.Equal("NewStr", (await reader.Objects[(3,0)].DirectValueAsync()).ToString());
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
        01 {secondRefPos:X4}  00
        endstream
        startxref
        { trailerPos} 
        %%EOF
        """ );
        return mbs;
    }
}