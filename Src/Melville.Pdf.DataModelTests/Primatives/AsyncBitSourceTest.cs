using System.IO;
using System.IO.Pipelines;
using System.Threading.Tasks;
using Melville.Parsing.Streams;
using Melville.Pdf.LowLevel.Filters.Jpeg;
using Melville.Pdf.ReferenceDocuments.Utility;
using Xunit;

namespace Melville.Pdf.DataModelTests.Primatives;

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