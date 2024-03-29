﻿using System.IO;
using Melville.JBig2.Segments;

namespace Melville.JBig2.HuffmanTables;

internal enum HuffmanTableSelection : byte
{
    UserSupplied, B1,B2,B3,B4,B5,B6,B7,B8,B9,B10,B11,B12,B13,B14,B15
}

internal static class HuffmanTableSelector
{
    public static HuffmanTableSelection Select(
        int flags, int position, 
        HuffmanTableSelection sel0, 
        HuffmanTableSelection sel1, 
        HuffmanTableSelection sel3 = HuffmanTableSelection.UserSupplied,
        HuffmanTableSelection selOther = HuffmanTableSelection.UserSupplied) =>
        (flags.UnsignedInteger(position,3)) switch
        {
            0x00 => sel0,
            0x01 => sel1,
            0x03 => sel3,
            _=> selOther
        };
 
}

internal class StandardHuffmanTables
{
    public static readonly HuffmanLine[] B1 =
    {
        new(1, 0, 4, 0, 1),
        new(2, 0b10, 8, 16, 1),
        new(3, 0b110, 16, 272, 1),
        new(3, 0b111, 32, 65808, 1)
    };

    public static readonly HuffmanLine[] B2 =
    {
        new(1, 0, 0, 0, 0),
        new(2, 0b10, 0, 1, 0),
        new(3, 0b110, 0, 2, 0),
        new(4, 0b1110, 3, 3, 1),
        new(5, 0b11110, 6, 11, 1),
        new(6, 0b111110, 32, 75, 1),
        new(6, 0b111111, 0, int.MaxValue, 0)
    };

    public static readonly HuffmanLine[] B3 =
    {
        new(1, 0b0, 0, 0, 0),
        new(2, 0b10, 0, 1, 0),
        new(3, 0b110, 0, 2, 0),
        new(4, 0b1110, 3, 3, 1),
        new(5, 0b11110, 6, 11, 1),
        new(6, 0b111110, 0, int.MaxValue, 0),
        new(7, 0b1111110, 32, 75, 1),
        new(8, 0b11111110, 8, -256, 1),
        new(8, 0b11111111, 32, -257, -1)
    };

    public static readonly HuffmanLine[] B4 =
    {
        new(1, 0b0, 0, 1, 0),
        new(2, 0b10, 0, 2, 0),
        new(3, 0b110, 0, 3, 0),
        new(4, 0b1110, 3, 4, 1),
        new(5, 0b11110, 6, 12, 1),
        new(5, 0b11111, 32, 76, 1)
    };

    public static readonly HuffmanLine[] B5 =
    {
        new(1, 0b0, 0, 1, 0),
        new(2, 0b10, 0, 2, 0),
        new(3, 0b110, 0, 3, 0),
        new(4, 0b1110, 3, 4, 1),
        new(5, 0b11110, 6, 12, 1),
        new(6, 0b111110, 32, 76, 1),
        new(7, 0b1111110, 8, -255, 1),
        new(7, 0b1111111, 32, -256, -1)
    };

    public static readonly HuffmanLine[] B6 =
    {
        new(2, 0b00, 7, 0, 1),
        new(3, 0b010, 7, 128, 1),
        new(3, 0b011, 8, 256, 1),
        new(4, 0b1100, 9, 512, 1),
        new(4, 0b1101, 10, 1024, 1),
        new(4, 0b1000, 9, -1024, 1),
        new(4, 0b1001, 8, -512, 1),
        new(4, 0b1010, 7, -256, 1),
        new(4, 0b1011, 5, -32, 1),
        new(5, 0b11101, 6, -128, 1),
        new(5, 0b11110, 5, -64, 1),
        new(5, 0b11100, 10, -2048, 1),
        new(6, 0b111110, 32, -2049, -1),
        new(6, 0b111111, 32, 2048, 1)
    };

    public static readonly HuffmanLine[] B7 =
    {
        new(3, 0b000, 8, -512, 1),
        new(3, 0b001, 8, 256, 1),
        new(3, 0b010, 9, 512, 1),
        new(3, 0b011, 10, 1024, 1),
        new(4, 0b1000, 9, -1024, 1),
        new(4, 0b1001, 7, -256, 1),
        new(4, 0b1010, 5, -32, 1),
        new(4, 0b1011, 5, 0, 1),
        new(4, 0b1100, 7, 128, 1),
        new(5, 0b11010, 6, -128, 1),
        new(5, 0b11011, 5, -64, 1),
        new(5, 0b11100, 5, 32, 1),
        new(5, 0b11101, 6, 64, 1),
        new(5, 0b11100, 5, 32, 1),
        new(5, 0b11101, 6, 64, 1),
        new(5, 0b11110, 32, -1025, -1),
        new(5, 0b11111, 32, 2048, 1)
    };

    public static readonly HuffmanLine[] B8 =
    {
        new(2, 0b00, 1, 0, 1),
        new(2, 0b01, 0, int.MaxValue, 0),
        new(3, 0b100, 4, 4, 1),
        new(4, 0b1010, 0, -1, 0),
        new(4, 0b1011, 4, 22, 1),
        new(4, 0b1100, 5, 38, 1),
        new(5, 0b11010, 0, 2, 0),
        new(5, 0b11011, 6, 70, 1),
        new(5, 0b11100, 7, 134, 1),
        new(6, 0b111010, 0, 3, 0),
        new(6, 0b111100, 7, 262, 1),
        new(6, 0b111101, 10, 646, 1),
        new(6, 0b111011, 1, 20, 1),
        new(7, 0b1111100, 0, -2, 0),
        new(7, 0b1111101, 8, 390, 1),
        new(8, 0b11111100, 3, -15, 1),
        new(8, 0b11111101, 1, -5, 1),
        new(9, 0b111111100, 1, -7, 1),
        new(9, 0b111111101, 0, -3, 0),
        new(9, 0b111111110, 32, -16, -1),
        new(9, 0b111111111, 32, 1670, 1)
    };

    public static readonly HuffmanLine[] B9 =
    {
        new(2, 0b00, 0, int.MaxValue, 0),
        new(3, 0b010, 1, -1, 1),
        new(3, 0b011, 1, 1, 1),
        new(3, 0b100, 5, 7, 1),
        new(4, 0b1010, 1, -3, 1),
        new(4, 0b1011, 5, 43, 1),
        new(4, 0b1100, 6, 75, 1),
        new(5, 0b11010, 1, 3, 1),
        new(5, 0b11011, 7, 139, 1),
        new(5, 0b11100, 8, 267, 1),
        new(6, 0b111010, 1, 5, 1),
        new(6, 0b111011, 2, 39, 1),
        new(6, 0b111100, 8, 523, 1),
        new(6, 0b111101, 11, 1291, 1),
        new(7, 0b1111100, 1, -5, 1),
        new(7, 0b1111101, 9, 779, 1),
        new(8, 0b11111100, 4, -31, 1),
        new(8, 0b11111101, 2, -11, 1),
        new(9, 0b111111100, 2, -15, 1),
        new(9, 0b111111101, 1, -7, 1),
        new(9, 0b111111110, 32, -32, -1),
        new(9, 0b111111111, 32, 3339, 1)
    };

    public static readonly HuffmanLine[] B10 =
    {
        new(2, 0b00, 2, -2, 1),
        new(2, 0b01, 6, 6, 1),
        new(2, 0b10, 0, int.MaxValue, 0),
        new(5, 0b11000, 0, -3, 0),
        new(5, 0b11001, 0, 2, 0),
        new(5, 0b11010, 5, 70, 1),
        new(6, 0b110110, 0, 3, 0),
        new(6, 0b110111, 5, 102, 1),
        new(6, 0b111000, 6, 134, 1),
        new(6, 0b111001, 7, 198, 1),
        new(6, 0b111010, 8, 326, 1),
        new(6, 0b111011, 9, 582, 1),
        new(6, 0b111100, 10, 1094, 1),
        new(7, 0b1111010, 4, -21, 1),
        new(7, 0b1111011, 0, -4, 0),
        new(7, 0b1111100, 0, 4, 0),
        new(7, 0b1111101, 11, 2118, 1),
        new(8, 0b11111100, 0, -5, 0),
        new(8, 0b11111101, 0, 5, 0),
        new(8, 0b11111110, 32, -22, -1),
        new(8, 0b11111111, 32, 4166, 1)
    };

    public static readonly HuffmanLine[] B11 =
    {
        new(1, 0b0, 0, 1, 0),
        new(2, 0b10, 1, 2, 1),
        new(4, 0b1100, 0, 4, 0),
        new(4, 0b1101, 1, 5, 1),
        new(5, 0b11100, 1, 7, 1),
        new(5, 0b11101, 2, 9, 1),
        new(6, 0b111100, 2, 13, 1),
        new(7, 0b1111010, 2, 17, 1),
        new(7, 0b1111011, 3, 21, 1),
        new(7, 0b1111100, 4, 29, 1),
        new(7, 0b1111101, 5, 45, 1),
        new(7, 0b1111110, 6, 77, 1),
        new(7, 0b1111111, 32, 141, 1)
    };

    public static readonly HuffmanLine[] B12 = {
        new(1, 0b0, 0, 1, 0),
        new(2, 0b10, 0, 2, 0),
        new(3, 0b110, 1, 3, 1),
        new(5, 0b11100, 0, 5, 0),
        new(5, 0b11101, 1, 6, 1),
        new(6, 0b111100, 1, 8, 1),
        new(7, 0b1111010, 0, 10, 0),
        new(7, 0b1111011, 1, 11, 1),
        new(7, 0b1111100, 2, 13, 1),
        new(7, 0b1111101, 3, 17, 1),
        new(7, 0b1111110, 4, 25, 1),
        new(8, 0b11111110, 5, 41, 1),
        new(8, 0b11111111, 32, 73, 1)
};

public static readonly HuffmanLine[] B13 = {
        new(1, 0b0, 0,1,0),
        new(3, 0b100, 0, 2, 0),
        new(3, 0b101, 3, 7, 1),
        new(4, 0b1100, 0, 3, 0),
        new(4, 0b1101, 1, 5, 1),
        new(5, 0b11100, 0, 4, 1),
        new(6, 0b111010, 1, 15, 1),
        new(6, 0b111011, 2, 17, 1),
        new(6, 0b111100, 3, 21, 1),
        new(6, 0b111101, 4, 29, 1),
        new(6, 0b111110, 5, 45, 1),
        new(7, 0b1111110, 6, 77, 1),
        new(7, 0b1111111, 32, 141, 1)
    };

    public static readonly HuffmanLine[] B14 = {
        new(1, 0b0, 0, 0, 0),
        // Notice this definition differs from table B14 in the spec, but is more efficient.
        new(2, 0b10, 1, -2, 1),
        new(2, 0b11, 1, 1, 1)
    };

    public static readonly HuffmanLine[] B15 = {
        // this differs from the spec table b15, but exploits more redundency
        new(1, 0b0, 0, 0,0),
        new(2, 0b10, 1, -1, 2),
        new(3, 0b110, 1, -2, 4),
        new(5, 0b11100, 1, -4, 1),
        new(5, 0b11101, 1, 3, 1),
        new(6, 0b111100, 2, -8, 1),
        new(6, 0b111101, 2, 5, 1),
        new(7, 0b1111100, 4, -24, 1),
        new(7, 0b1111101, 4, 9, 1),
        new(7, 0b1111110, 32, -25, -1),
        new(7, 0b1111111, 32, 25, 1)
    };

    public static HuffmanLine[] ArrayFromSelector(HuffmanTableSelection tableSelector) => tableSelector switch
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
        HuffmanTableSelection.B10 => B10,
        HuffmanTableSelection.B11 => B11,
        HuffmanTableSelection.B12 => B12,
        HuffmanTableSelection.B13 => B13,
        HuffmanTableSelection.B14 => B14,
        HuffmanTableSelection.B15 => B15,
        _ => throw new InvalidDataException("Cannot find standard huffman table: " + tableSelector)
    };
}

internal static class DirectBitstreamReaders
{
    public static readonly HuffmanLine[] ZeroBit = { new(0, 0, 0, 0, 0) };
    public static readonly HuffmanLine[] OneBit = { new(0, 0, 1, 0, 1) };
    public static readonly HuffmanLine[] TwoBit = { new(0, 0, 2, 0, 1) };
    public static readonly HuffmanLine[] ThreeBit = { new(0, 0, 3, 0, 1) };

    public static HuffmanLine[] FromBitLength(int logSize) => logSize switch
    {
        0 => ZeroBit,
        1 => OneBit,
        2 => TwoBit,
        3 => ThreeBit,
        _=> new HuffmanLine[]{new (0,0, logSize, 0, 1)}
    };
}