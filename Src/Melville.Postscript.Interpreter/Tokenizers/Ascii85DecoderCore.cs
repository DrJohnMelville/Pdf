using Melville.StreamInterfaces.ParsingBuffers;
using System;
using System.Diagnostics;

namespace Melville.Postscript.Interpreter.Tokenizers;

/// <summary>
/// Ascii85 parsing method
/// </summary>
public ref struct Ascii85DecoderCore()
{
    private uint buffer = 0;
    private int bytesRead = 0;
    private int bytesToWrite = 0;

    /// <summary>
    /// Retrieve a memento from this struct
    /// </summary>
    /// <returns>the memmento</returns>
    public (uint, int, int) GetMemento() => (buffer, bytesRead, bytesToWrite);

    /// <summary>
    /// Create a new parser from a memmento
    /// </summary>
    /// <param name="memento">the memmento fro this item</param>
    public Ascii85DecoderCore((uint, int, int) memento) : this()
    {
        (buffer, bytesRead, bytesToWrite) = memento;
    }

    /// <summary>
    /// Convert as much input as possible to the output.
    /// </summary>
    /// <param name="input">The input bytes</param>
    /// <param name="output">buffer to receive the output -- the method will advance this.</param>
    /// <param name="final">True if the buffer is all the data there is, false if there is more to be read</param>
    /// <returns>True if the output represents the entire string, false otherwise</returns>
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
            output[0] = (byte)(buffer >> 8 * bytesToWrite);
            output = output[1..];
        }
        buffer = 0;
        bytesRead = 0;
        return true;
    }
}

