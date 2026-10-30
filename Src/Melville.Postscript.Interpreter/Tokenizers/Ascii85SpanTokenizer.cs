using Melville.Parsing;
using Melville.Parsing.IStreamAdaptors;
using Melville.Postscript.Interpreter.Values;
using Melville.StreamInterfaces.ParsingBuffers;
using System;
using System.Diagnostics;

namespace Melville.Postscript.Interpreter.Tokenizers;

internal sealed class Ascii85SpanTokenizer : SpanTokenizer
{
    protected override bool TryFindLength(ReadOnlySpan<byte> source, out int length)
    {
        length = source.IndexOf("~>"u8) + 2;
        return length > 2;
    }
    protected override PostscriptValue DecodeSpan(ReadOnlySpan<byte> source)
    {
        var input = new SpanParsingBuffer(source[2..^2]);
        var buffer = new PooledBuffer<byte>(source.Length);
        var destination = buffer.AsSpan();
        var core = new Ascii85DecoderCore();
        while (!core.InnerConvert(ref input, ref destination, true))
        {
            var len = GetLength(buffer, destination);
            var newBuffer = new PooledBuffer<byte>(buffer.Length * 2);
            buffer.AsSpan().CopyTo(newBuffer.AsSpan());
            destination = newBuffer.AsSpan()[len..];
            buffer.Dispose();
            buffer = newBuffer;
        }
        ;
        var length = 
            GetLength(buffer, destination);
        var ret = PostscriptValueFactory.CreateString(buffer.AsSpan()[..length], StringKind.String);
        buffer.Dispose();
        return ret;
    }

    private static int GetLength(PooledBuffer<byte> buffer, Span<byte> destination) =>
        buffer.Length - destination.Length;

}

public ref struct Ascii85DecoderCore()
{
    private uint buffer = 0;
    private int bytesRead = 0;
    private int bytesToWrite = 0;

    public (uint, int, int) GetMemento() => (buffer, bytesRead, bytesToWrite);
    public Ascii85DecoderCore((uint, int, int) memento) : this()
    {
        (buffer, bytesRead, bytesToWrite) = memento;
    }

    public bool InnerConvert(ref SpanParsingBuffer input, ref Span<byte> output, bool final)
    {
        while (true)
        {
            if (bytesToWrite > 0)
            {
                if (!TryWriteBuffer(ref output)) return false;
            }
            else
            {
                if (!TryReadCode(ref input))
                {
                    if (!final) return false;
                    if (bytesRead is 0) return true;
                    PushPartialByte();
                }
            }

        }
    }
    void PushPartialByte()
    {
        Debug.Assert(bytesRead > 1);
        bytesToWrite = bytesRead - 1;
        for (; bytesRead < 5; bytesRead++) PushByte(Ascii85Constants.IncompleteGroupPadding);
        for (int i = 3; i >= bytesToWrite; i--) buffer >>= 8;
        bytesRead = 0;
    }
    bool TryReadCode(ref SpanParsingBuffer input)
    {
        Debug.Assert(bytesToWrite == 0);
        for (; bytesRead < 5; bytesRead++)
        {
            if (!input.TryAdvanceToExcept(CharacterClassifier.WhitespaceSearchValues)) return false;
            var b = input.Peek()[0];
            if (b == Ascii85Constants.FirstTerminatingChar) return false;
            input.Advance(1);
            if (b == Ascii85Constants.FourZeroChar)
                ExpandAsZeros();
            else
                PushByte(b);
        }
        Debug.Assert(bytesRead == 5);
        bytesToWrite = 4;
        bytesRead = 0;
        return true;
    }
    void ExpandAsZeros()
    {
        Debug.Assert(buffer is 0);
        Debug.Assert(bytesRead is 0);
        buffer = 0;
        bytesRead = 4;
    }
    void PushByte(byte b)
    {
        Debug.Assert((uint)bytesRead < 5);
        buffer *= 85u;
        buffer += (uint)(b - Ascii85Constants.FirstChar);
    }
    bool TryWriteBuffer(ref Span<byte> output)
    {
        Debug.Assert(bytesRead == 0);
        while (bytesToWrite > 0)
        {
            if (output.Length is 0) return false;
            bytesToWrite--;
            output[0] = (byte)(buffer >> (8 * bytesToWrite));
            output = output[1..];
        }
        buffer = 0;
        bytesRead = 0;
        return true;
    }
}