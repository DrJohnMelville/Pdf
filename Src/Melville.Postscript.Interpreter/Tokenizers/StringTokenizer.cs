using System;
using System.Buffers;
using System.Diagnostics.CodeAnalysis;
using Melville.Postscript.Interpreter.Values;

namespace Melville.Postscript.Interpreter.Tokenizers;


/// <summary>
/// Reads a string from a SequenceReader using a StringDecoder to do the decoding
/// </summary>
/// <typeparam name="T">The type of string decoder to use</typeparam>
/// <typeparam name="TState">The type of the string decoder's state.</typeparam>
public readonly struct StringTokenizer<T, TState>
    where T : IStringDecoder<TState>, new() where TState : new()
{
    /// <summary>
    /// Parse a SequenceReaded containing a string to a PostScriptValue for that string
    /// </summary>
    /// <param name="reader">The reader to draw from</param>
    /// <param name="value">Receives the finished value</param>
    /// <returns>True if successful, false if not enough characters are found</returns>
    public bool Parse(ref SequenceReader<byte> reader, out PostscriptValue value)
    {
        if (!TryCountChars(reader, out var length))
            return default(PostscriptValue).AsFalseValue(out value);
        if (length > PostscriptString.ShortStringLimit)
            CreateLongString(ref reader, length, out value);
        else
            CreateShortString(ref reader, length, out value);
        return true;
    }

    /// <summary>
    /// Parse a string to a byte array
    /// </summary>
    /// <param name="reader">The reader to get bytes from</param>
    /// <param name="value">Out variable that receives the output</param>
    /// <returns>True if there are enough CodeSource characters to return a value,
    /// false otherwise.</returns>
    public bool Parse(
        ref SequenceReader<byte> reader, [NotNullWhen(true)]out byte[]? value)
    {
        if (!TryCountChars(reader, out var length))
            return ((byte[])null!).AsFalseValue(out value);
        value = new byte[length];
        FillBuffer(ref reader, value.AsSpan());
        return true;
    }

    // Note intentionally not a ref reader, because we want a copy
    private bool TryCountChars(SequenceReader<byte> reader, out int length)
    {
        var decoder = new T();
        var state = new TState();
        Span<byte> buffer = stackalloc byte[decoder.MaxCharsPerBlock];
        length = 0;
        while (true)
        {
            switch (decoder.DecodeFrom(ref reader, buffer, ref state))
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
        var state = new TState();
        var localTarget = target;
        while (true)
        {
            switch (decoder.DecodeFrom(ref reader, localTarget, ref state))
            {
                case <1: return;
                case var length:
                    localTarget = localTarget.Slice(length);
                    break;
            }
        }
    }
}