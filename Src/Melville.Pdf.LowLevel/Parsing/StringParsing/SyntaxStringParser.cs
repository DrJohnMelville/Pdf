using System.Buffers;
using System.Diagnostics.CodeAnalysis;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Parsing.ObjectParsers;

namespace Melville.Pdf.LowLevel.Parsing.StringParsing
{
    public  class SyntaxStringParser: PdfAtomParser
    {
        public override bool TryParse(
            ref SequenceReader<byte> input, bool final, [NotNullWhen(true)] out PdfObject? output)
        {
            var copyOfInput = input;
            if (!ComputeStringLength(ref input, out var length))
            {
                if (final)
                {
                    throw new PdfParseException("Unterminated Syntax string");
                }
                output = null;
                return false;
            }
            output = CreateString(ref copyOfInput, length);
            return true;
        }

        private static bool ComputeStringLength(ref SequenceReader<byte> input, out int length)
        {
            length = 0;
            var stateMachine = new SyntaxStringStateMachine();
            while (true)
            {
                switch (stateMachine.TryOneParse(ref input, out var item))
                {
                    case SyntaxStringResult.EndOfString:
                        return true;
                    case SyntaxStringResult.EndOfInput:
                        return false;
                }
                length++;
            }
        }

        private static PdfString CreateString(ref SequenceReader<byte> input, int length)
        {
            var buf = new byte[length];
            int bufpos = 0;
            var stateMachine = new SyntaxStringStateMachine();
            while (true)
            {
                if (stateMachine.TryOneParse(ref input, out var item) == SyntaxStringResult.EndOfString) 
                    return new PdfString(buf);
                buf[bufpos++] = item;
            }
  
        }
    }
}