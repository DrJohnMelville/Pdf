using System.Buffers;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace Melville.JpegLibrary.HuffmanCoding;

internal class JpegHuffmanDecodingTable
{
    internal JpegHuffmanDecodingTable(byte tableClass, byte identifier)
    {
        TableClass = tableClass;
        Identifier = identifier;
    }

    public byte TableClass { get; }
    public byte Identifier { get; }

    /// <summary>
    /// Derived from the DHT marker. Contains the symbols, in order of incremental code length.
    /// </summary>
    private byte[]? _values;

    /// <summary>
    /// Contains the largest code of length k (0 if none). MaxCode[17] is a sentinel to ensure the decoder terminates.
    /// </summary>
    private ushort[]? _maxCode;

    /// <summary>
    /// Contains the largest code of length k (0 if none). MaxCode[17] is a sentinel to ensure the decoder terminates.Values[] offset for codes of length k  ValOffset[k] = Values[] index of 1st symbol of code length k, less the smallest code of length k; so given a code of length k, the corresponding symbol is Values[code + ValOffset[k]].
    /// </summary>
    private byte[]? _valOffset;

    private Entry[]? _lookaheadTable;

    /// <summary>
    /// A Huffman symbol.
    /// </summary>
    public struct Entry
    {
        public byte CodeSize { get; set; }
        public byte SymbolValue { get; set; }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Entry Lookup(int code16bit)
    {
        Debug.Assert(_lookaheadTable is not null);

        int high8 = code16bit >> 8;
        Entry entry = _lookaheadTable![high8];
        if (entry.CodeSize != 0)
        {
            return entry;
        }

        return LookupSlow(code16bit);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private Entry LookupSlow(int code16bit)
    {
        Debug.Assert(_maxCode is not null);
        Debug.Assert(_values is not null);
        Debug.Assert(_valOffset is not null);

        ushort[] maxCode = _maxCode!;

        int size = 9;
        while (code16bit > maxCode[size])
        {
            size++;
        }

        if (size > 16)
        {
            throw new InvalidDataException("Invalid Huffman code encountered.");
        }

        code16bit >>= (16 - size);
        return new Entry
        {
            CodeSize = (byte)size,
            SymbolValue = _values![(_valOffset![size] + code16bit) & 0xFF]
        };
    }

    public static bool TryParse(ReadOnlySequence<byte> buffer, [NotNullWhen(true)] out JpegHuffmanDecodingTable? huffmanTable, out int bytesConsumed)
    {
        if (buffer.IsSingleSegment)
        {
            return TryParse(buffer.FirstSpan, out huffmanTable, out bytesConsumed);
        }

        bytesConsumed = 0;
        if (buffer.IsEmpty)
        {
            huffmanTable = null;
            return false;
        }
        byte tableClassAndIdentifier = buffer.FirstSpan[0];
        bytesConsumed++;

        return TryParse((byte)(tableClassAndIdentifier >> 4), (byte)(tableClassAndIdentifier & 0xf), buffer.Slice(1), out huffmanTable, ref bytesConsumed);
    }

    public static bool TryParse(ReadOnlySpan<byte> buffer, [NotNullWhen(true)] out JpegHuffmanDecodingTable? huffmanTable, out int bytesConsumed)
    {
        bytesConsumed = 0;
        if (buffer.IsEmpty)
        {
            huffmanTable = null;
            return false;
        }

        byte tableClassAndIdentifier = buffer[0];
        bytesConsumed++;

        return TryParse((byte)(tableClassAndIdentifier >> 4), (byte)(tableClassAndIdentifier & 0xf), buffer.Slice(1), out huffmanTable, ref bytesConsumed);
    }

    [SkipLocalsInit]
    public static bool TryParse(byte tableClass, byte identifier, ReadOnlySequence<byte> buffer, [NotNullWhen(true)] out JpegHuffmanDecodingTable? huffmanTable, ref int bytesConsumed)
    {
        if (buffer.IsSingleSegment)
        {
            return TryParse(tableClass, identifier, buffer.FirstSpan, out huffmanTable, ref bytesConsumed);
        }

        if (buffer.Length < 16)
        {
            huffmanTable = null;
            return false;
        }

        Span<byte> codeLengths = stackalloc byte[16];
        buffer.Slice(0, 16).CopyTo(codeLengths);

        int codeCount = 0;
        for (int i = 15; i >= 0; i--)
        {
            codeCount += codeLengths[i];
        }
        if (codeCount > 256)
        {
            huffmanTable = null;
            return false;
        }

        // Generate code size table
        Span<byte> huffSize = stackalloc byte[256 + 1];
        GenerateSizeTable(codeLengths, huffSize);
        buffer = buffer.Slice(16);
        bytesConsumed += 16;

        if (buffer.Length < codeCount)
        {
            huffmanTable = default;
            return false;
        }

        // Generate code values table
        Span<ushort> huffCode = stackalloc ushort[257];
        GenerateCodeTable(huffSize, huffCode);
        bytesConsumed += codeCount;

        // Configure huffman table
        Span<byte> codeValues = stackalloc byte[codeCount];
        buffer.Slice(0, codeCount).CopyTo(codeValues);

        huffmanTable = new JpegHuffmanDecodingTable(tableClass, identifier);
        huffmanTable.Configure(codeLengths, huffCode, codeValues);

        return true;
    }

    [SkipLocalsInit]
    public static bool TryParse(byte tableClass, byte identifier, ReadOnlySpan<byte> buffer, [NotNullWhen(true)] out JpegHuffmanDecodingTable? huffmanTable, ref int bytesConsumed)
    {
        if (buffer.Length < 16)
        {
            huffmanTable = null;
            return false;
        }

        int codeCount = 0;
        for (int i = 15; i >= 0; i--)
        {
            codeCount += buffer[i];
        }
        if (codeCount > 256)
        {
            huffmanTable = null;
            return false;
        }

        // Generate code size table
        ReadOnlySpan<byte> codeLengths = buffer.Slice(0, 16);
        Span<byte> huffSize = stackalloc byte[256 + 1];
        GenerateSizeTable(codeLengths, huffSize);
        buffer = buffer.Slice(16);
        bytesConsumed += 16;

        if (buffer.Length < codeCount)
        {
            huffmanTable = null;
            return false;
        }

        // Generate code values table
        Span<ushort> huffCode = stackalloc ushort[257];
        GenerateCodeTable(huffSize, huffCode);
        bytesConsumed += codeCount;

        // Configure huffman table
        huffmanTable = new JpegHuffmanDecodingTable(tableClass, identifier);
        huffmanTable.Configure(codeLengths, huffCode, buffer.Slice(0, codeCount));

        return true;
    }

    private static int GenerateSizeTable(ReadOnlySpan<byte> bits, Span<byte> huffSize)
    {
        Debug.Assert(bits.Length == 16);
        Debug.Assert(huffSize.Length == 257);

        int k = 0;
        for (int i = 1; i <= 16; i++)
        {
            int j = 1;
            while (j++ <= bits[i - 1])
            {
                huffSize[k++] = (byte)i;
            }
        }
        huffSize[k] = 0;
        return k;
    }

    private static void GenerateCodeTable(ReadOnlySpan<byte> huffSize, Span<ushort> huffCode)
    {
        Debug.Assert(huffSize.Length == 257);

        int k = 0;
        int code = 0;
        int si = huffSize[0];

        while (true)
        {
            do
            {
                huffCode[k] = (ushort)code;
                code++;
                k++;
            } while (huffSize[k] == si);
            if (huffSize[k] == 0)
            {
                return;
            }
            do
            {
                code <<= 1;
                si++;
            } while (huffSize[k] != si);
        }
    }

    private void Configure(ReadOnlySpan<byte> codeLengths, ReadOnlySpan<ushort> huffCode, ReadOnlySpan<byte> values)
    {
        _values = new byte[256];
        _maxCode = new ushort[18];
        _valOffset = new byte[19];
        _lookaheadTable = new Entry[256];

        values.CopyTo(_values);

        int p = 0;
        for (int l = 1; l <= 16; l++)
        {
            if (codeLengths[l - 1] != 0)
            {
                int offset = p - huffCode[p];
                _valOffset[l] = (byte)offset;
                p += codeLengths[l - 1];
                _maxCode[l] = huffCode[p - 1];
                _maxCode[l] <<= 16 - l;
                _maxCode[l] = (ushort)(_maxCode[l] | (uint)((1 << (16 - l)) - 1));
            }
            else
            {
                _maxCode[l] = 0;
            }
        }
        _valOffset[18] = 0;
        _maxCode[17] = ushort.MaxValue;

        p = 0;
        for (int l = 1; l <= 8; l++)
        {
            for (int i = 0; i < codeLengths[l - 1]; i++, p++)
            {
                FillByteLookupTable(huffCode[p], (byte)l, _values[p]);
            }
        }
    }

    private void FillByteLookupTable(int code, byte codeSize, byte value)
    {
        Debug.Assert(_lookaheadTable is not null);
        Debug.Assert(codeSize <= 8);

        Entry[] table = _lookaheadTable!;
        int freeBitCount = 8 - codeSize;
        code = (byte)(code << freeBitCount);
        for (int i = 0; i < (1 << freeBitCount); i++)
        {
            table[code + i] = new Entry { CodeSize = codeSize, SymbolValue = value };
        }
    }
}