using System.Buffers;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using Melville.Pdf.LowLevel.Model;

namespace Melville.Pdf.LowLevel.Parsing.StringParsing
{
    public static class LiteralStringParser
    {
        public static bool TryParse(
            ref SequenceReader<byte> input, [NotNullWhen(true)] out PdfString? output)
        {
            var copyOfInput = input;
            if (!ComputeStringLength(ref input, out var length))
            {
                output = null;
                return false;
            }
            output = CreateString(ref copyOfInput, length);
            return true;
        }

        private static bool ComputeStringLength(ref SequenceReader<byte> input, out int length)
        {
            var byteCounter = new ByteCounter();
            var stateMachine = new LiteralStringParserStateMachine(byteCounter);
            if (!stateMachine.TryParse(ref input))
            {
                length = 0;
                return false;
            }
            
            length = byteCounter.Length;
            return true;
        }

        private static PdfString CreateString(ref SequenceReader<byte> input, int length)
        {
            var ret = new ArrayBuilder(length);
            var stateMachine = new LiteralStringParserStateMachine(ret);
            var result = stateMachine.TryParse(ref input);
            Debug.Assert(result);
            return new PdfString(ret.Result);
        }
    }
}