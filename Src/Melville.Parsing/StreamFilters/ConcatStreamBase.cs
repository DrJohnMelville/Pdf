using System.Diagnostics.CodeAnalysis;
using Melville.Parsing.AwaitConfiguration;
using Melville.Parsing.Streams.Bases;

namespace Melville.Parsing.StreamFilters
{

    /// <summary>
    /// Expresses a sequence streams as a sinngle concatenated stream.
    /// </summary>
    public abstract class ConcatStreamBase : DefaultBaseStream
    {
        private Stream? current = null;

        /// <summary>
        /// Create a ConcatenatedStreamBase
        /// </summary>
        protected ConcatStreamBase(): base(true, false, false)
        {
            current = this;
        }


        /// <inheritdoc />
        public override async ValueTask<int> ReadAsync(
            Memory<byte> buffer, CancellationToken cancellationToken = default)
        {
            await TryLoadFirstSourceAsync().CA();
            var bytesWritten = 0;
            while (!AtEndOfStream() && bytesWritten < buffer.Length)
            {
                var localWritten = await current.ReadAsync(buffer[bytesWritten..], cancellationToken).CA();
                bytesWritten += localWritten;
                await PrepareForNextReadAsync(localWritten).CA();
            }

            return bytesWritten;
        }

        private async ValueTask TryLoadFirstSourceAsync()
        {
            // we use this as a sentinel to mean we have not gotten the first source yet
            current = (current == this) ? await GetNextStreamAsync().CA(): current;
        }
    
        [MemberNotNullWhen(false, nameof(current))]
        private bool AtEndOfStream() => current is null;

        private async ValueTask PrepareForNextReadAsync(int localWritten)
        {
            if (PriorReadSucceeded(localWritten)) return;
            if (current != null) await current.DisposeAsync().CA();
            current = await GetNextStreamAsync().CA();;
        }

        private static bool PriorReadSucceeded(int localWritten) => localWritten > 0;

        /// <inheritdoc />
        protected override void Dispose(bool disposing)
        {
            current?.Dispose();
            base.Dispose(disposing);
        }

        /// <summary>
        /// Get the next stream in the sequence.
        /// </summary>
        /// <returns>The next stream in the sequence, or null if the sequence is over.</returns>
        protected abstract ValueTask<Stream?> GetNextStreamAsync();
    }
}