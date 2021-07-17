using System;
using System.Buffers;
using System.IO.Pipelines;
using System.Threading.Tasks;
using Melville.Pdf.LowLevel.Parsing.ParserContext;

namespace Melville.Pdf.LowLevel.Parsing.ObjectParsers
{
    public static class TokenChecker
    {
        public static async ValueTask<bool> CheckToken(IParsingReader reader, byte[] template)
        {
            bool result = false;
            do
            {
            } while (reader.ShouldContinue(VerifyTag(await reader.ReadAsync(), template, out result)));

            return result;
        }

        private static (bool Success, SequencePosition Position) VerifyTag(
            ReadResult source, byte[] template, out bool result)
        {
            var reader = new SequenceReader<byte>(source.Buffer);
            return (reader.TryCheckToken(template, out result), reader.Position);
        }

        public static bool TryCheckToken(
            this ref SequenceReader<byte> input, byte[] template, out bool result)
        {
            result = false;
            foreach (var expected in template)
            {
                if (!input.TryRead(out var actual)) return false;
                if (expected != actual) return true;
            }
            result = true;
            return true;
        }
    }
}