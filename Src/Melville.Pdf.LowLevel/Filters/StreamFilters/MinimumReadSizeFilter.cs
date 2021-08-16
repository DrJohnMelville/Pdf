using System;
using System.Buffers;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Melville.Pdf.LowLevel.Filters.StreamFilters
{
    // Many of the filters, like ASCII85Decode and RLE can produce more than one
    // byte of output at a certian point in the input.  This wrapper class guarentees that the reads
    // to a class will be at least a minimum size, this means that the inner streams do not need to
    // worry about dividing the output accross multiple reads, because you will eventually get a buffer
    // big enough to hold the entire unit.
    public class MinimumReadSizeFilter : SequentialReadFilterStream
    {
        private readonly Stream source;
        private readonly int minReadSize;
        private byte[]? priorData;
        private int priorDataStart;
        private int priorDataLength;
        private int UnusedPriorDataLength() => priorDataLength - priorDataStart;

        public MinimumReadSizeFilter(Stream source, int minReadSize)
        {
            this.source = source;
            this.minReadSize = minReadSize;
        }

        public override ValueTask<int> ReadAsync(
            Memory<byte> buffer, CancellationToken cancellationToken = default)
        {
            if (HasPriorData()) return ReadFromPriorData(buffer, cancellationToken);
            if (buffer.Length < minReadSize) return ReadWithTemporaryBuffer(buffer, cancellationToken);
            return ReadIntoBuffer(buffer, cancellationToken);
        }

        [MemberNotNullWhen(true)]
        private bool HasPriorData() => priorData != null;

        private async ValueTask<int> ReadWithTemporaryBuffer(
            Memory<byte> buffer, CancellationToken cancellationToken)
        {
            Debug.Assert(!HasPriorData());
            priorData = ArrayPool<byte>.Shared.Rent(minReadSize + 1);
            priorDataStart = 0;
            priorDataLength = await ReadIntoBuffer(this.priorData.AsMemory(), cancellationToken);
            return await ReadFromPriorData(buffer, cancellationToken);
        }

        private async ValueTask<int> ReadFromPriorData(Memory<byte> buffer, CancellationToken cancellationToken)
        {
            Debug.Assert(HasPriorData());
            var bytesToCopy = Math.Min(buffer.Length, UnusedPriorDataLength());
            CopyFromTempBuffer(buffer, bytesToCopy);
            if (ResidueToSmallForAnotherRead(buffer, bytesToCopy)) return bytesToCopy;
            return bytesToCopy + await ReadIntoBuffer(buffer.Slice(bytesToCopy), cancellationToken);
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

        private ValueTask<int> ReadIntoBuffer(Memory<byte> buffer, CancellationToken cancellationToken) => 
            source.ReadAsync(buffer, cancellationToken);

        protected override void Dispose(bool disposing)
        {
            source.Dispose();
            base.Dispose(disposing);
        }
    }
}