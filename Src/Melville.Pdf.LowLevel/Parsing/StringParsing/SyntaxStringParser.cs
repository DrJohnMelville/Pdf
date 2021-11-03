using System.Buffers;
using System.Diagnostics.CodeAnalysis;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Model.Primitives;
using Melville.Pdf.LowLevel.Parsing.ObjectParsers;
using Melville.Pdf.LowLevel.Parsing.ParserContext;

namespace Melville.Pdf.LowLevel.Parsing.StringParsing;

public  class SyntaxStringParser: PdfAtomParser
{
    public override bool TryParse(
        ref SequenceReader<byte> input, bool final, IParsingReader source, 
        [NotNullWhen(true)] out PdfObject? output)
    {
        var outputBytes = TryParseToBytes(ref input, final);
        if (outputBytes == null)
        {
            output = null;
            return false;
        }
        output = source.CreateDecryptedString(outputBytes);
        return true;
    }

    public static byte[]? TryParseToBytes(ref SequenceReader<byte> input, bool final)
    {
        var copyOfInput = input;
        if (!ComputeStringLength(ref input, out var length))
        {
            if (final)
            {
                throw new PdfParseException("Unterminated Syntax string");
            }
            return null;
        }
        return CreateString(ref copyOfInput, length);
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

    private static byte[] CreateString(
        ref SequenceReader<byte> input, int length)
    {
        var buf = new byte[length];
        int bufpos = 0;
        var stateMachine = new SyntaxStringStateMachine();
        while (true)
        {
            if (stateMachine.TryOneParse(ref input, out var item) == SyntaxStringResult.EndOfString)
                return buf;
            buf[bufpos++] = item;
        }
  
    }
}