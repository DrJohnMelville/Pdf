using System;
using System.Buffers;
using Melville.Postscript.Interpreter.Values;

namespace Melville.Postscript.Interpreter.Tokenizers;

/// <summary>
/// Represents string decoding strategies
/// </summary>
internal interface IStringDecoder
{
    /// <summary>
    /// Decode a number of bytes from the source to the destination
    /// </summary>
    /// <param name="source">source of bytes to decode</param>
    /// <param name="destination">span to write to</param>
    /// <returns>positive number of bytes written, 0 if stream is ended,
    /// -1 if more bytes are needed.</returns>
    public int DecodeFrom(ref SequenceReader<byte> source, scoped Span<byte> destination);

    /// <summary>
    /// Gets the maximum number of characters that could be written in a single call
    /// to DecodeFrom
    /// </summary>
    int MaxCharsPerBlock { get; }
}

internal readonly struct StringTokenizer<T> where T: IStringDecoder, new()
{
    public bool Parse(ref SequenceReader<byte> reader, out PostscriptValue value)
    {
        if (!TryCountChars(reader, out var length))
            return default(PostscriptValue).AsFalseValue(out value);
        if (length > IByteStringSource.ShortStringLimit)
            CreateLongString(ref reader, length, out value);
        else
            CreateShortString(ref reader, length, out value);
        return true;
    }

    // Note intentionally not a ref reader, because we want a copy
    private bool TryCountChars(SequenceReader<byte> reader, out int length)
    {
        var decoder = new T();
        Span<byte> buffer = stackalloc byte[decoder.MaxCharsPerBlock];
        length = 0;
        while (true)
        {
            switch (decoder.DecodeFrom(ref reader, buffer))
            {
                case -1: return false;
                case 0: return true;
                case var bytes:
                    length += bytes;
                    break;
            }
        }
    }

    private void CreateLongString(
        ref SequenceReader<byte> reader, int length, out PostscriptValue value)
    {
        var buffer = new byte[length];
        FillBuffer(ref reader, buffer.AsSpan());
        value = PostscriptValueFactory.CreateString(buffer, StringKind.String);
    }

    private void CreateShortString(
        ref SequenceReader<byte> reader, int length, out PostscriptValue value)
    {
        Span<byte> buffer = stackalloc byte[length];
        FillBuffer(ref reader, buffer);
        value = PostscriptValueFactory.CreateString(buffer, StringKind.String);
    }

    private void FillBuffer(ref SequenceReader<byte> reader, scoped Span<byte> target)
    {
        var decoder = new T();
        var localTarget = target;
        while (true)
        {
            switch (decoder.DecodeFrom(ref reader, localTarget))
            {
                case <1: return;
                case var length:
                    localTarget = localTarget.Slice(length);
                    break;
            }
        }
    }
}

internal readonly struct Ascii85Decoder : IStringDecoder
{
    public int DecodeFrom(ref SequenceReader<byte> source, scoped Span<byte> destination)
    {
        if (!source.TryReadNextVisible(out var byte1)) return -1;
        return byte1 switch
        {
            Ascii85Constants.FirstTerminatingChar => 0,
            Ascii85Constants.FourZeroChar => FillZeroBytes(destination),
            _ => DecodeQuintuple(ref source, destination, byte1)
        };
    }

    private int FillZeroBytes(Span<byte> destination)
    {
        destination[..4].Clear();
        return 4;
    }

    private int DecodeQuintuple(
        ref SequenceReader<byte> source, scoped Span<byte> destination, byte firstChar)
    {
        var quint = new Ascii85Quintuple();
        quint.AddByte(firstChar);
        for (int i = 0; i < 4; i++)
        {
            if (!source.TryReadNextVisible(out var nextChar)) return -1;
            if (nextChar is Ascii85Constants.FirstTerminatingChar)
            {
                source.Rewind(1);
                return quint.WriteDecodedBytesPartial(destination);
            }
            quint.AddByte(nextChar);
        }

        return quint.WriteDecodedBytes(destination, 4);
    }

    public int MaxCharsPerBlock => 4;
}

internal ref struct Ascii85Quintuple
{
    private uint data = 0;
    private int sourceBytes = 0;

    public Ascii85Quintuple()
    {
    }

    public void AddByte(byte b)
    {
        data *= 85u;
        data += (uint)(b - Ascii85Constants.FirstChar);
        sourceBytes++;
    }
    
    public int WriteDecodedBytesPartial(scoped Span<byte> destination)
    {
        int originalBytes = sourceBytes;
        PadForMissingBytes();
        var bytesToWrite = originalBytes -1;
        WasteExtraBytes(bytesToWrite);
        return WriteDecodedBytes(destination, bytesToWrite);
    }

    private void PadForMissingBytes()
    {
        if (sourceBytes < 1) throw new PostscriptParseException("Invalid Ascii85 string");
        while (sourceBytes < 5) AddByte(Ascii85Constants.IncompleteGroupPadding);
    }

    private void WasteExtraBytes(int bytesToWrite)
    {
        for (int i = bytesToWrite; i < 4; i++)
        {
            data >>= 8;
        }
    }

    public int WriteDecodedBytes(scoped Span<byte> destination, int bytesToWrite)
    {
        for (int i = bytesToWrite - 1; i >= 0; i--)
        {
            destination[i] = (byte)(data & 0xFF);
            data >>= 8;
        }

        return bytesToWrite;
    }
}

internal static class Ascii85Constants
{
    public const byte FirstChar = (byte)'!';
    public const byte IncompleteGroupPadding = (byte)'u';
    public const byte FirstTerminatingChar = (byte)'~';
    public const byte SecondTerminatingChar = (byte)'>';
    public const byte FourZeroChar = (byte)'z';
}
