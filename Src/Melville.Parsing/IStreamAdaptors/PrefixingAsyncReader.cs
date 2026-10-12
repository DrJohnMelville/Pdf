using Melville.StreamInterfaces;
using System;

namespace Melville.Parsing.IStreamAdaptors;

/// <summary>
/// This is a AsyncReader that has a prefix Memory that will be returned before the 
/// underlying reader.
/// </summary>
/// <param name="inner">The reader to get data from.</param>
/// <param name="prefix">Data to prefix ahead of the inner reader</param>
public class PrefixingAsyncReader(IAsyncReader inner, byte[] prefix) : IAsyncReader
{
    private Memory<byte> prefix = prefix.AsMemory();

    ///<inheritdoc/>
    public ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellation = default)
    {
        if (prefix.Length > 0)
        {
            var len = Math.Min(prefix.Length, buffer.Length);
            prefix.Span[..len].CopyTo(buffer.Span);
            prefix = prefix[len..];
            return ValueTask.FromResult(len);
        }
        return inner.ReadAsync(buffer, cancellation);
    }
}