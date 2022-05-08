using System.IO;
using Melville.Pdf.LowLevel.Filters.CCITTFaxDecodeFilters;

namespace Melville.Pdf.LowLevel.Filters.Jbig2Filter.HuffmanTables;

public enum HuffmanTableSelection : byte
{
    UserSupplied, B1,B2,B3,B4,B5,B6,B7,B8,B9,B10,B11,B12,B13,B14,B15
}
public class StandardHuffmanTables
{
    private static readonly HuffmanTable B1 = new HuffmanTable(
        new HuffmanLine(1,0,4,0, 1),
        new HuffmanLine(2,0b10, 8, 16, 1),
        new HuffmanLine(3,0b110, 16,272, 1),
        new HuffmanLine(3,0b111, 32, 65808, 1)
    );

    private static readonly HuffmanTable B2 = new(
        new HuffmanLine(1,0,0,0,0),
        new HuffmanLine(2, 0b10, 0, 1, 0),
        new HuffmanLine(3, 0b110, 0, 2, 0),
        new HuffmanLine(4, 0b1110, 3, 3,1),
        new HuffmanLine(5, 0b11110, 6, 11, 1),
        new HuffmanLine(6, 0b111110, 32, 75, 1 ),
        new HuffmanLine(6, 0b111111, 0, int.MaxValue, 0)
    );

    private static readonly HuffmanTable B3 = new(
        new HuffmanLine(1,0b0, 0, 0, 0),
        new HuffmanLine(2, 0b10, 0, 1, 0),
        new HuffmanLine(3, 0b110, 0, 2, 0),
        new HuffmanLine(4, 0b1110, 3, 3, 1),
        new HuffmanLine(5, 0b11110, 6,11, 1),
        new HuffmanLine(6, 0b111110, 0, int.MaxValue, 0),
        new HuffmanLine(7, 0b1111110, 32, 75, 1),
        new HuffmanLine(8, 0b11111110, 8, -256, 1),
        new HuffmanLine(8, 0b11111111, 32, -257, -1)
        );

    private static readonly HuffmanTable B4 = new(
        new HuffmanLine(1, 0b0, 0, 1, 0),
        new HuffmanLine(2, 0b10, 0, 2, 0),
        new HuffmanLine(3, 0b110, 0, 3, 0),
        new HuffmanLine(4, 0b1110, 3, 4, 1),
        new HuffmanLine(5, 0b11110, 6, 12, 1),
        new HuffmanLine(5, 0b11111, 32, 76, 1)
        );

    private static readonly HuffmanTable B5 = new(
        new HuffmanLine(1, 0b0, 0, 1, 0),
        new HuffmanLine(2, 0b10, 0, 2, 0),
        new HuffmanLine(3, 0b110, 0, 3, 0),
        new HuffmanLine(4, 0b1110, 3, 4, 1),
        new HuffmanLine(5, 0b11110, 6, 12, 1),
        new HuffmanLine(6, 0b111110, 32, 76, 1),
        new HuffmanLine(7, 0b1111110, 8,-255,1),
        new HuffmanLine(7, 0b1111111, 32, -256,-1)
        );

    private static readonly HuffmanTable B6 = new(
        new HuffmanLine(2, 0b00, 7, 0, 1),
        new HuffmanLine(3, 0b010, 7, 128, 1),
        new HuffmanLine(3, 0b011, 8, 256, 1),
        new HuffmanLine(4, 0b1100, 9, 512, 1),
        new HuffmanLine(4, 0b1101, 10, 1024, 1),
        new HuffmanLine(4, 0b1000, 9, -1024, 1),
        new HuffmanLine(4, 0b1001, 8, -512, 1),
        new HuffmanLine(4, 0b1010, 7, -256, 1),
        new HuffmanLine(5, 0b11101, 6, -128, 1),
        new HuffmanLine(5, 0b11110, 5, -64, 1),
        new HuffmanLine(5, 0b11100, 10, -2048, 1),
        new HuffmanLine(6, 0b111110, 32, -2049, -1),
        new HuffmanLine(6, 0b111111, 32, 2048, 1)
    );

    private static readonly HuffmanTable B7 = new(
        new HuffmanLine(3, 0b000, 8, -512, 1),
        new HuffmanLine(3, 0b001, 8, 256, 1),
        new HuffmanLine(3, 0b010, 9, 512, 1),
        new HuffmanLine(3, 0b011, 10, 1024, 1),
        new HuffmanLine(4, 0b1000, 9, -1024, 1),
        new HuffmanLine(4, 0b1001, 7, -256, 1),
        new HuffmanLine(4, 0b1010, 5, -32, 1),
        new HuffmanLine(4, 0b1011, 5, 0, 1),
        new HuffmanLine(4, 0b1100, 7, 128, 1),
        new HuffmanLine(5, 0b11010, 6, -128, 1),
        new HuffmanLine(5, 0b11011, 5,-64, 1),
        new HuffmanLine(5, 0b11100, 5, 32, 1),
        new HuffmanLine(5, 0b11101, 6, 64, 1),
        new HuffmanLine(5, 0b11100, 5, 32, 1),
        new HuffmanLine(5, 0b11101, 6, 64, 1),
        new HuffmanLine(5, 0b11110, 32, -1025, -1),
        new HuffmanLine(5, 0b11111, 32, 2048, 1)
    );

    private static readonly HuffmanTable B8 = new(
        new HuffmanLine(2, 0b00, 1, 0, 1),
        new HuffmanLine(2, 0b01, 0, int.MaxValue, 0),
        new HuffmanLine(4, 0b1010, 0, -1, 0),
        new HuffmanLine(4, 0b1011, 4, 22, 1),
        new HuffmanLine(4, 0b1100, 5, 38, 1),
        new HuffmanLine(5, 0b11010, 0, 2, 0),
        new HuffmanLine(5, 0b11011, 6, 70, 1),
        new HuffmanLine(5, 0b11100, 7, 134, 1),
        new HuffmanLine(6, 0b111100, 7, 262, 1),
        new HuffmanLine(6, 0b111101, 10, 646, 1),
        new HuffmanLine(7, 0b1111100, 0, -2, 0),
        new HuffmanLine(8, 0b11111100, 3, -15, 1),
        new HuffmanLine(8, 0b11111101, 1, -5, 1),
        new HuffmanLine(9, 0b111111101, 0, -3, 0),
        new HuffmanLine(9, 0b111111110, 32, -16, -1),
        new HuffmanLine(9, 0b111111111, 32, 1670, 1)
    );

    private static readonly HuffmanTable B9 = new(
        new HuffmanLine(2, 0b00, 0, int.MaxValue, 0),
        new HuffmanLine(3, 0b010, 1, -1, 1),
        new HuffmanLine(3, 0b011, 1, 1, 1),
        new HuffmanLine(3, 0b100,  5,7, 1),
        new HuffmanLine(4, 0b1010, 1, -3, 1),
        new HuffmanLine(4, 0b1011, 5, 43, 1),
        new HuffmanLine(4, 0b1100, 6, 75, 1),
        new HuffmanLine(5, 0b11010, 1, 3, 1),
        new HuffmanLine(5, 0b11011, 7, 139, 1),
        new HuffmanLine(5, 0b11100, 8, 267, 1),
        new HuffmanLine(6, 0b111011, 2, 39, 1),
        new HuffmanLine(6, 0b111100, 8, 523, 1),
        new HuffmanLine(6, 0b111101, 11, 1291, 1),
        new HuffmanLine(7, 0b1111100, 1, -5, 1),
        new HuffmanLine(7, 0b1111101, 9, 779, 1),
        new HuffmanLine(8, 0b11111100, 4, -31, 1),
        new HuffmanLine(8, 0b11111101, 2, -11, 1),
        new HuffmanLine(9, 0b111111101, 1, -7, 1),
        new HuffmanLine(9, 0b111111110, 32, -32, -1),
        new HuffmanLine(9, 0b111111111, 32, 3339, 1)
    );

    public static HuffmanTable FromSelector(HuffmanTableSelection tableSelector) => tableSelector switch
    {
        HuffmanTableSelection.B1 => B1,
        HuffmanTableSelection.B2 => B2,
        HuffmanTableSelection.B3 => B3,
        HuffmanTableSelection.B4 => B4,
        HuffmanTableSelection.B5 => B5,
        HuffmanTableSelection.B6 => B6,
        HuffmanTableSelection.B7 => B7,
        HuffmanTableSelection.B8 => B8,
        HuffmanTableSelection.B9 => B9,
        _ => throw new InvalidDataException("Cannot find standard huffman table: " + tableSelector)
    };
}