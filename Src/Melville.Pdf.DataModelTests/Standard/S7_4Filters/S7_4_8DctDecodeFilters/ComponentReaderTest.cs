using System;
using System.IO;
using System.IO.Pipelines;
using System.Linq;
using System.Threading.Tasks;
using Melville.Hacks.Reflection;
using Melville.Pdf.LowLevel.Filters.Jpeg;
using Melville.Pdf.ReferenceDocuments.Utility;
using Xunit;

namespace Melville.Pdf.DataModelTests.Standard.S7_4Filters.S7_4_8DctDecodeFilters;

public class ComponentReaderTest
{
    [Theory]
    [InlineData(1, 1, 1)]
    [InlineData(1, 0, -1)]
    [InlineData(2, 0, -3)]
    [InlineData(2, 1, -2)]
    [InlineData(2, 2, 2)]
    [InlineData(2, 3, 3)]
    [InlineData(3, 0, -7)]
    [InlineData(3, 1, -6)]
    [InlineData(3, 2, -5)]
    [InlineData(3, 3, -4)]
    [InlineData(3, 4, 4)]
    [InlineData(3, 5, 5)]
    [InlineData(3, 6, 6)]
    [InlineData(3, 7,7)]
    [InlineData(5, 1, -30)]
    public void DecodeNumberTest(int len, int code, int value) =>
        Assert.Equal(value, ComponentReader.DecodeNumber(len, code));

    [Fact]
    public async Task ParseSampleBitstream()
    {
        var sut = new ComponentReader(new ComponentDefinition(ComponentId.Y, 1, 1),
            acHuffmanTable, dcHuffman);

        await sut.ReadMcuAsync(
            new AsyncBitSource(
                new AsyncByteSource(
                    PipeReader.Create(new MemoryStream( 
                        //DC -30   (0,2) (0,-5)  (1,-2)   (0,1)  (0,-2)  (2,1)  (3,1)   (3,1)    EOB  
                        "110_00001 01_10 100_010 11011_01 00_1   01_01   11100_1 1110101 1110101 1010".BitsFromBinary()
                        )))));

        Assert.Equal(new int[]
        {
            -30,  2,  1, -2, 0, 0, 0, 0,
            -5, -2,  0,  1, 0, 0, 0, 0,
            0,  0,  0,  1, 0, 0, 0, 0,
            1,  0,  0,  0, 0, 0, 0, 0,
            0,  0,  0,  0, 0, 0, 0, 0,
            0,  0,  0,  0, 0, 0, 0, 0,
            0,  0,  0,  0, 0, 0, 0, 0,
            0,  0,  0,  0, 0, 0, 0, 0
        }, (int[])sut.GetField("mcuValues")!);
    }

    private string Format8x8(int[] values) =>
        string.Join("\r\n", values.Chunk(8)
            .Select(i => string.Join(", ", 
                i.Select(j => j.ToString("000;-00;000")))));

    private static readonly HuffmanTable dcHuffman = new(new HuffmanLine[]
    {
        new(2, 0b000, 00),
        new(3, 0b010, 01),
        new(3, 0b011, 02),
        new(3, 0b100, 03),
        new(3, 0b101, 04),
        new(3, 0b110, 05),
        new(4, 0b1110, 06),
        new(5, 0b11110, 07),
        new(6, 0b111110, 08),
        new(7, 0b1111110, 09),
        new(8, 0b11111110, 10),
        new(9, 0b111111110, 11),
    });

    private static readonly HuffmanTable acHuffmanTable = new(new HuffmanLine[]
    {
        new(2, 0b00, 0x01),
        new(2, 0b01, 0x02),
        new(3, 0b100, 0x03),
        new(4, 0b1010, 0x00),
        new(4, 0b1011, 0x04),
        new(4, 0b1100, 0x11),
        new(5, 0b11010, 0x05),
        new(5, 0b11011, 0x12),
        new(5, 0b11100, 0x21),
        new(6, 0b111010, 0x31),
        new(6, 0b111011, 0x41),
        new(7, 0b1111000, 0x06),
        new(7, 0b1111001, 0x13),
        new(7, 0b1111010, 0x51),
        new(7, 0b1111011, 0x61),
        new(8, 0b11111000, 0x07),
        new(8, 0b11111001, 0x22),
        new(8, 0b11111010, 0x71),
        new(9, 0b111110110, 0x14),
        new(9, 0b111110111, 0x32),
        new(9, 0b111111000, 0x81),
        new(9, 0b111111001, 0x91),
        new(10, 0b1111110110, 0x08),
        new(10, 0b1111110111, 0x23),
        new(10, 0b1111111000, 0x42),
        new(11, 0b11111110110, 0x15),
        new(11, 0b11111110111, 0x52),
        new(12, 0b111111110100, 0x24),
        new(12, 0b111111110101, 0x33),
        new(12, 0b111111110110, 0x62),
        new(12, 0b111111110111, 0x72),
        new(15, 0b111111111000000, 0x82),
        new(16, 0b1111111110000010, 0x09),
        new(16, 0b1111111110000100, 0x16),
        new(16, 0b1111111110000101, 0x17),
        new(16, 0b1111111110000110, 0x18),
        new(16, 0b1111111110000111, 0x19),
        new(16, 0b1111111110001001, 0x25),
        new(16, 0b1111111110001010, 0x26),
        new(16, 0b1111111110001011, 0x27),
        new(16, 0b1111111110001100, 0x28),
        new(16, 0b1111111110001101, 0x29),
        new(16, 0b1111111110001111, 0x34),
        new(16, 0b1111111110010000, 0x35),
        new(16, 0b1111111110010001, 0x36),
        new(16, 0b1111111110010010, 0x37),
        new(16, 0b1111111110010011, 0x38),
        new(16, 0b1111111110010100, 0x39),
        new(16, 0b1111111110010110, 0x43),
        new(16, 0b1111111110010111, 0x44),
        new(16, 0b1111111110011000, 0x45),
        new(16, 0b1111111110011001, 0x46),
        new(16, 0b1111111110011010, 0x47),
        new(16, 0b1111111110011011, 0x48),
        new(16, 0b1111111110011100, 0x49),
        new(16, 0b1111111110011110, 0x53),
        new(16, 0b1111111110011111, 0x54),
        new(16, 0b1111111110100000, 0x55),
        new(16, 0b1111111110100001, 0x56),
        new(16, 0b1111111110100010, 0x57),
        new(16, 0b1111111110100011, 0x58),
        new(16, 0b1111111110100100, 0x59),
        new(16, 0b1111111110100110, 0x63),
        new(16, 0b1111111110100111, 0x64),
        new(16, 0b1111111110101000, 0x65),
        new(16, 0b1111111110101001, 0x66),
        new(16, 0b1111111110101010, 0x67),
        new(16, 0b1111111110101011, 0x68),
        new(16, 0b1111111110101100, 0x69),
        new(16, 0b1111111110101110, 0x73),
        new(16, 0b1111111110101111, 0x74),
        new(16, 0b1111111110110000, 0x75),
        new(16, 0b1111111110110001, 0x76),
        new(16, 0b1111111110110010, 0x77),
        new(16, 0b1111111110110011, 0x78),
        new(16, 0b1111111110110100, 0x79),
        new(16, 0b1111111110110110, 0x83),
        new(16, 0b1111111110110111, 0x84),
        new(16, 0b1111111110111000, 0x85),
        new(16, 0b1111111110111001, 0x86),
        new(16, 0b1111111110111010, 0x87),
        new(16, 0b1111111110111011, 0x88),
        new(16, 0b1111111110111100, 0x89),
        new(16, 0b1111111110111110, 0x92),
        new(16, 0b1111111110111111, 0x93),
        new(16, 0b1111111111000000, 0x94),
        new(16, 0b1111111111000001, 0x95),
        new(16, 0b1111111111000010, 0x96),
        new(16, 0b1111111111000011, 0x97),
        new(16, 0b1111111111000100, 0x98),
        new(16, 0b1111111111000101, 0x99)
    });

}