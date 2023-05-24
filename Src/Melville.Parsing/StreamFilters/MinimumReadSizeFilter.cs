using System.Buffers;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using Melville.Parsing.AwaitConfiguration;
using Melville.Parsing.Streams.Bases;

namespace Melville.Parsing.StreamFilters;

/// <summary>
/// Many of the filters, like ASCII85Decode and RLE can produce more than one
/// byte of output at a certian point in the input.  This wrapper class guarentees that the reads
/// to a class will be at least a minimum size, this means that the inner streams do not need to
/// worry about dividing the output across multiple reads, because you will eventually get a buffer
/// big enough to hold the entire unit.
/// </summary>
public class MinimumReadSizeFilter : DefaultBaseStream
{
    private readonly Stream source;
    private readonly int minReadSize;
    private byte[]? priorData;
    private int priorDataStart;
    private int priorDataLength;
    private int UnusedPriorDataLength() => priorDataLength - priorDataStart;

    /// <summary>
    /// Create a minimumReadSizeStream
    /// </summary>
    /// <param name="source">Source to read data from</param>
    /// <param name="minReadSize">Smallest legal read buffer size.</param>
    public MinimumReadSizeFilter(Stream source, int minReadSize): base(true, false, false)
    {
        this.source = source;
        this.minReadSize = minReadSize;
    }

    /// <inheritdoc />
    public override ValueTask<int> ReadAsync(
        Memory<byte> buffer, CancellationToken cancellationToken = default)
    {
        if (HasPriorData()) return ReadFromPriorDataAsync(buffer, cancellationToken);
        if (buffer.Length < minReadSize) return ReadWithTemporaryBufferAsync(buffer, cancellationToken);
        return ReadIntoBufferAsync(buffer, cancellationToken);
    }

    [MemberNotNullWhen(true)]
    private bool HasPriorData() => priorData != null;

    private async ValueTask<int> ReadWithTemporaryBufferAsync(
        Memory<byte> buffer, CancellationToken cancellationToken)
    {
        Debug.Assert(!HasPriorData());
        priorData = ArrayPool<byte>.Shared.Rent(minReadSize + 1);
        priorDataStart = 0;
        priorDataLength = await ReadIntoBufferAsync(this.priorData.AsMemory(), cancellationToken).CA();
        return await ReadFromPriorDataAsync(buffer, cancellationToken).CA();
    }

    private async ValueTask<int> ReadFromPriorDataAsync(Memory<byte> buffer, CancellationToken cancellationToken)
    {
        Debug.Assert(HasPriorData());
        var bytesToCopy = Math.Min(buffer.Length, UnusedPriorDataLength());
        CopyFromTempBuffer(buffer, bytesToCopy);
        if (ResidueToSmallForAnotherRead(buffer, bytesToCopy)) return bytesToCopy;
        return bytesToCopy + await ReadIntoBufferAsync(buffer.Slice(bytesToCopy), cancellationToken).CA();
    }

    private bool ResidueToSmallForAnotherRead(Memory<byte> buffer, int bytesUsed) => 
        buffer.Length - bytesUsed < minReadSize;

    private void CopyFromTempBuffer(Memory<byte> buffer, int bytesToCopy)
    {
        priorData.AsSpan(priorDataStart, bytesToCopy).CopyTo(buffer.Span);
        TryReleaseTemporaryBuffer(bytesToCopy);
    }

    private void TryReleaseTemporaryBuffer(int bytesUsed)
    {
        Debug.Assert(priorData != null);
        priorDataStart += bytesUsed;
        if (UnusedPriorDataLength() > 0) return;
        ArrayPool<byte>.Shared.Return(priorData);
        priorData = null;
    }

    private ValueTask<int> ReadIntoBufferAsync(Memory<byte> buffer, CancellationToken cancellationToken) => 
        source.ReadAsync(buffer, cancellationToken);

    /// <inheritdoc />
    protected override void Dispose(bool disposing)
    {
        source.Dispose();
        base.Dispose(disposing);
    }

    /// <inheritdoc />
    public async override ValueTask DisposeAsync()
    {
        await source.DisposeAsync().CA();
        await base.DisposeAsync().CA();
    }
}