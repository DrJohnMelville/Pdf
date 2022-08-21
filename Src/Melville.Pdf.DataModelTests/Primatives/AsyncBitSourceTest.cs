using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Pipelines;
using System.Linq;
using System.Threading.Tasks;
using Melville.Parsing.Streams;
using Melville.Pdf.LowLevel.Filters.Jpeg;
using Melville.Pdf.ReferenceDocuments.Utility;
using Xunit;

namespace Melville.Pdf.DataModelTests.Primatives;

public class JpegPixelBufferTest
{
    private static readonly int[] deZagged = new[]
    {
        0,  1,  5,  6, 14, 15, 27, 28,
        2,  4,  7, 13, 16, 26, 29, 42,
        3,  8, 12, 17, 25, 30, 41, 43,
        9, 11, 18, 24, 31, 40, 44, 53,
       10, 19, 23, 32, 39, 45, 52, 54,
       20, 22, 33, 38, 46, 51, 55, 60,
       21, 34, 37, 47, 50, 56, 59, 61,
       35, 36, 48, 49, 57, 58, 62, 63
    };
    private static readonly byte[] rowOfZeros = new byte[24];


    private static void ZizZagTest(int componentIndex, Func<int, IEnumerable<byte>> componentSel, int mcu, Func<byte[], IEnumerable<byte>> zeroOffsetter)
    {
        var buffer = new JpegPixelBuffer(16, 3);
        var basePt = buffer.McuStart(mcu).Slice(componentIndex);
        var offsets = buffer.ZizZagDecodingOffsets();
        for (int i = 0; i < 63; i++)
        {
            basePt.Span[offsets[i]] = (byte)(i + 1);
        }
        var result = deZagged
            .SelectMany(componentSel)
            .Chunk(24)
            .Select(zeroOffsetter)
            .SelectMany(i => i)
            .ToArray();
        Assert.Equal(result, buffer.McuStart(0).Span.ToArray());
    }
}

public class AsyncBitSourceTest
{
    private static async Task<AsyncBitSource> CreateReader(string BinaryData)
    {
        var source = BinaryData.BitsFromBinary();
        var reader = PipeReader.Create(new OneCharAtAtimeStream(source));
        var bitReader = await AsyncBitSource.Create(reader);
        return bitReader;
    }

    [Fact]
    public async Task Read5BitGroups()
    {
        var bitReader = await CreateReader("00000 11111 01010 00011");
        Assert.Equal(0b00000u, await bitReader.ReadBitsAsync(5));
        Assert.Equal(0b11111u, await bitReader.ReadBitsAsync(5));
        Assert.Equal(0b01010u, await bitReader.ReadBitsAsync(5));
        Assert.Equal(0b00011u, await bitReader.ReadBitsAsync(5));
    }

    [Fact]
    public async Task ReadBitsWhileFilterintOut00()
    {
        var bitReader = await CreateReader("10100000 00000000 11111111 00000000");
        Assert.Equal(1, await bitReader.ReadBitAsync());
        Assert.Equal(0, await bitReader.ReadBitAsync());
        Assert.Equal(1, await bitReader.ReadBitAsync());
        Assert.Equal(0, await bitReader.ReadBitAsync());
        Assert.Equal(0, await bitReader.ReadBitAsync());
        Assert.Equal(0, await bitReader.ReadBitAsync());
        Assert.Equal(0, await bitReader.ReadBitAsync());
        Assert.Equal(0, await bitReader.ReadBitAsync());
     
        Assert.Equal(1, await bitReader.ReadBitAsync());
        Assert.Equal(1, await bitReader.ReadBitAsync());
        Assert.Equal(1, await bitReader.ReadBitAsync());
        Assert.Equal(1, await bitReader.ReadBitAsync());
        Assert.Equal(1, await bitReader.ReadBitAsync());
        Assert.Equal(1, await bitReader.ReadBitAsync());
        Assert.Equal(1, await bitReader.ReadBitAsync());
        Assert.Equal(1, await bitReader.ReadBitAsync());
        
        Assert.Equal(0, await bitReader.ReadBitAsync());
        Assert.Equal(0, await bitReader.ReadBitAsync());
        Assert.Equal(0, await bitReader.ReadBitAsync());
        Assert.Equal(0, await bitReader.ReadBitAsync());
        Assert.Equal(0, await bitReader.ReadBitAsync());
        Assert.Equal(0, await bitReader.ReadBitAsync());
        Assert.Equal(0, await bitReader.ReadBitAsync());
        Assert.Equal(0, await bitReader.ReadBitAsync());
    }
    [Fact]
    public async Task FilterInitial00()
    {
        var bitReader = await CreateReader("00000000 11111111 00000000");
     
        Assert.Equal(1, await bitReader.ReadBitAsync());
        Assert.Equal(1, await bitReader.ReadBitAsync());
        Assert.Equal(1, await bitReader.ReadBitAsync());
        Assert.Equal(1, await bitReader.ReadBitAsync());
        Assert.Equal(1, await bitReader.ReadBitAsync());
        Assert.Equal(1, await bitReader.ReadBitAsync());
        Assert.Equal(1, await bitReader.ReadBitAsync());
        Assert.Equal(1, await bitReader.ReadBitAsync());
        
        Assert.Equal(0, await bitReader.ReadBitAsync());
        Assert.Equal(0, await bitReader.ReadBitAsync());
        Assert.Equal(0, await bitReader.ReadBitAsync());
        Assert.Equal(0, await bitReader.ReadBitAsync());
        Assert.Equal(0, await bitReader.ReadBitAsync());
        Assert.Equal(0, await bitReader.ReadBitAsync());
        Assert.Equal(0, await bitReader.ReadBitAsync());
        Assert.Equal(0, await bitReader.ReadBitAsync());
    }
    [Fact]
    public async Task ReadTerminal00()
    {
        var bitReader = await CreateReader("10100000 00000000");
        Assert.Equal(1, await bitReader.ReadBitAsync());
        Assert.Equal(0, await bitReader.ReadBitAsync());
        Assert.Equal(1, await bitReader.ReadBitAsync());
        Assert.Equal(0, await bitReader.ReadBitAsync());
        Assert.Equal(0, await bitReader.ReadBitAsync());
        Assert.Equal(0, await bitReader.ReadBitAsync());
        Assert.Equal(0, await bitReader.ReadBitAsync());
        Assert.Equal(0, await bitReader.ReadBitAsync());
        
        Assert.Equal(0, await bitReader.ReadBitAsync());
        Assert.Equal(0, await bitReader.ReadBitAsync());
        Assert.Equal(0, await bitReader.ReadBitAsync());
        Assert.Equal(0, await bitReader.ReadBitAsync());
        Assert.Equal(0, await bitReader.ReadBitAsync());
        Assert.Equal(0, await bitReader.ReadBitAsync());
        Assert.Equal(0, await bitReader.ReadBitAsync());
        Assert.Equal(0, await bitReader.ReadBitAsync());
    }

    [Fact]
    public async Task ReadSomeBits()
    {
        var bitReader = await CreateReader("1010 0000 11110000 11111111 00000000");
        Assert.Equal(1, await bitReader.ReadBitAsync());
        Assert.Equal(0, await bitReader.ReadBitAsync());
        Assert.Equal(1, await bitReader.ReadBitAsync());
        Assert.Equal(0, await bitReader.ReadBitAsync());
        Assert.Equal(0, await bitReader.ReadBitAsync());
        Assert.Equal(0, await bitReader.ReadBitAsync());
        Assert.Equal(0, await bitReader.ReadBitAsync());
        Assert.Equal(0, await bitReader.ReadBitAsync());
        
        Assert.Equal(1, await bitReader.ReadBitAsync());
        Assert.Equal(1, await bitReader.ReadBitAsync());
        Assert.Equal(1, await bitReader.ReadBitAsync());
        Assert.Equal(1, await bitReader.ReadBitAsync());
        Assert.Equal(0, await bitReader.ReadBitAsync());
        Assert.Equal(0, await bitReader.ReadBitAsync());
        Assert.Equal(0, await bitReader.ReadBitAsync());
        Assert.Equal(0, await bitReader.ReadBitAsync());
        
        Assert.Equal(1, await bitReader.ReadBitAsync());
        Assert.Equal(1, await bitReader.ReadBitAsync());
        Assert.Equal(1, await bitReader.ReadBitAsync());
        Assert.Equal(1, await bitReader.ReadBitAsync());
        Assert.Equal(1, await bitReader.ReadBitAsync());
        Assert.Equal(1, await bitReader.ReadBitAsync());
        Assert.Equal(1, await bitReader.ReadBitAsync());
        Assert.Equal(1, await bitReader.ReadBitAsync());
        
        Assert.Equal(0, await bitReader.ReadBitAsync());
        Assert.Equal(0, await bitReader.ReadBitAsync());
        Assert.Equal(0, await bitReader.ReadBitAsync());
        Assert.Equal(0, await bitReader.ReadBitAsync());
        Assert.Equal(0, await bitReader.ReadBitAsync());
        Assert.Equal(0, await bitReader.ReadBitAsync());
        Assert.Equal(0, await bitReader.ReadBitAsync());
        Assert.Equal(0, await bitReader.ReadBitAsync());
    }
}