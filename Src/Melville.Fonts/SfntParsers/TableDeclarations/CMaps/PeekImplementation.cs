using System.Buffers;
using System.IO.Pipelines;
using Melville.Parsing.AwaitConfiguration;
using Melville.Parsing.SequenceReaders;

namespace Melville.Fonts.SfntParsers.TableDeclarations.CMaps
{
    public static class PeekImplementation
    {
        public static async ValueTask<ulong> PeekTag(this PipeReader reader, int bytes)
        {
            var result = await reader.ReadAtLeastAsync(bytes).CA();
            var ret = PeekAtBytes(result.Buffer, bytes);
            reader.AdvanceTo(result.Buffer.Start, result.Buffer.GetPosition(bytes));
            return ret;
        }

        private static ulong PeekAtBytes(ReadOnlySequence<byte> resultBuffer, int bytes)
        {
            var reader = new SequenceReader<byte>(resultBuffer);
            return reader.ReadBigEndianUint(bytes);
        }
    }
}