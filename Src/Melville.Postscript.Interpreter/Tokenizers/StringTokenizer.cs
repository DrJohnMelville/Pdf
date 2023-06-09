using System;
using System.Buffers;
using Melville.Postscript.Interpreter.Values;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Melville.Postscript.Interpreter.Tokenizers;

/// <summary>
/// Represents string decoding strategies
/// </summary>
internal interface IStringDecoder<TState>
{
    /// <summary>
    /// Decode a number of bytes from the source to the destination
    /// </summary>
    /// <param name="source">source of bytes to decode</param>
    /// <param name="destination">span to write to</param>
    /// <param name="state">A state variable that will be maintained over successive calls
    /// in the same stream</param>
    /// <returns>positive number of bytes written, 0 if stream is ended,
    /// -1 if more bytes are needed.</returns>
    public int DecodeFrom(
        ref SequenceReader<byte> source, scoped Span<byte> destination, ref TState state);

    /// <summary>
    /// Gets the maximum number of characters that could be written in a single call
    /// to DecodeFrom
    /// </summary>
    int MaxCharsPerBlock { get; }
}

internal readonly struct StringTokenizer<T, TState>
    where T : IStringDecoder<TState>, new() where TState : new()
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