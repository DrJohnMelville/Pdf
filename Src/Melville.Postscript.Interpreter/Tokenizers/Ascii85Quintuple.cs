using System;
using Melville.Postscript.Interpreter.Values.Interfaces;

namespace Melville.Postscript.Interpreter.Tokenizers;

/// <summary>
/// This structure handles deconding ascii85 5byte groups into 4 byte ascii wroups
/// </summary>
public ref struct Ascii85Quintuple()
{
    private uint data = 0;

    /// <summary>
    /// Add a byte to the quintuple.
    /// </summary>
    /// <param name="b">the byte to add</param>
    public void AddByte(byte b)
    {
        data *= 85u;
        data += (uint)(b - Ascii85Constants.FirstChar);
    }

    /// <summary>
    /// Write the terminal portion of the encoding to the span.
    /// </summary>
    /// <param name="destination">destination of the decoded bytes</param>
    /// <param name="bytesPushed">number of source bytes pushed</param>
    /// <returns>The number of destination bytes written.</returns>
    public int WriteDecodedBytesPartial(scoped Span<byte> destination, int bytesPushed)
    {
        PadForMissingBytes(bytesPushed);
        var bytesToWrite = bytesPushed - 1;
        RemoveUnwrittenBytes(bytesToWrite);
        return WriteDecodedBytes(destination, bytesToWrite);
    }

    private void PadForMissingBytes(int sourceBytes)
    {
        if (sourceBytes < 2) 
            throw new PostscriptNamedErrorException("Invalid Ascii85 string","syntaxerror");
        for (int i = sourceBytes; i < 5; i++) 
            AddByte(Ascii85Constants.IncompleteGroupPadding);
    }

    private void RemoveUnwrittenBytes(int bytesToWrite)
    {
        for (int i = 3; i >= bytesToWrite; i--)
        {
            data >>= 8;
        }
    }

    /// <summary>
    /// Write a given number of output bytes to the destination
    /// </summary>
    /// <param name="destination">the span to write to</param>
    /// <param name="bytesToWrite">number of bytes to write</param>
    /// <returns>The number of bytes written.</returns>
    public int WriteDecodedBytes(scoped Span<byte> destination, int bytesToWrite)
    {

        for (int i = bytesToWrite -1; i >= 0; i--)
        {
            destination[i] = (byte)data;
            data >>= 8;
        }
        return bytesToWrite;
    }
}