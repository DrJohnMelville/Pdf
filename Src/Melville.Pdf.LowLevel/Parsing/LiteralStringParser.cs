using System;
using System.Buffers;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using Melville.Pdf.LowLevel.Model;

namespace Melville.Pdf.LowLevel.Parsing
{
    public static class LiteralStringParser
    {
        public static bool TryParse(
            ref SequenceReader<byte> input, [NotNullWhen(true)] out PdfString? output)
        {
            output = null;
            var copyOfInput = input;
            if (!ComputeStringLength(ref input, out var length)) return false;
            output = CreateString(ref copyOfInput, length);
            return true;
        }

        private static bool ComputeStringLength(ref SequenceReader<byte> input, out int length)
        {
            length = 0;
            var stateMachine = new LiteralStringParserStateMachine();
            while (true)
            {
                if (!input.TryRead(out var character)) return false;
                switch (stateMachine.PushByte(character, out var convertedCharacter))
                {
                    case LspResult.ProducedByte:
                        length++;
                        break;
                    case LspResult.Done:
                        return true;
                }
            }
        }

        private static PdfString CreateString(ref SequenceReader<byte> input, int length)
        {
            var ret = new byte[length];
            int bytesReturned = 0;
            var stateMachine = new LiteralStringParserStateMachine();
            while (true)
            {
                var readResult = input.TryRead(out var character);
                Debug.Assert(readResult); // then length checker did succeeded, so this should too
                switch (stateMachine.PushByte(character, out var convertedCharacter))
                {
                    case LspResult.ProducedByte:
                        ret[bytesReturned++] = convertedCharacter;
                        break;
                    case LspResult.Done:
                        return new PdfString(ret);
                }
            }
        }
    }

    public ref struct LiteralStringParserStateMachine
    {
        private LspState state;
        private int parenCount; 

        public LspResult PushByte(byte input, out byte output)
        {
            output = input;
            LspResult ret;
            (state, ret, parenCount, output) = Transition(state, parenCount, input);
            return ret;
        }

        private static (LspState, LspResult, int, byte) Transition(
            LspState state, int parenCount, byte input) => 
                 (state, parenCount, (char)input) switch
        {
            // count parentheses and kill the process when we get back to zero
            (LspState.Normal, 0, '(') => (LspState.Normal, LspResult.NoByte, 1, 0),
            (LspState.Normal, 1, ')') => (LspState.Done, LspResult.Done, 0, 0),
            
            //Internal balanced parens get reflected in the output
            (LspState.Normal, _, '(') => (LspState.Normal, LspResult.ProducedByte, parenCount+1, input),
            (LspState.Normal, _, ')') => (LspState.Normal, LspResult.ProducedByte, parenCount-1, input),
            
            //ReverseSolidus transitions to escape mode
            (LspState.Normal, _, '\\') => (LspState.Escaped, LspResult.NoByte, parenCount, 0),
            (LspState.Escaped, _, 'n') => (LspState.Normal, LspResult.ProducedByte, parenCount, (byte)'\n'),
            (LspState.Escaped, _, 'r') => (LspState.Normal, LspResult.ProducedByte, parenCount, (byte)'\r'),
            (LspState.Escaped, _, 't') => (LspState.Normal, LspResult.ProducedByte, parenCount, (byte)'\t'),
            (LspState.Escaped, _, 'b') => (LspState.Normal, LspResult.ProducedByte, parenCount, (byte)'\b'),
            (LspState.Escaped, _, 'f') => (LspState.Normal, LspResult.ProducedByte, parenCount, (byte)'\f'),
            
            //End oof line 
            (LspState.Normal, _, '\r' or '\n') => 
                (LspState.InsideEoln, LspResult.ProducedByte, parenCount, (byte)'\n'),
            (LspState.Escaped, _, '\r' or '\n') => 
                (LspState.InsideEoln, LspResult.NoByte, parenCount, 0),
            (LspState.InsideEoln, _, '\r' or '\n') =>
                (LspState.InsideEoln, LspResult.NoByte, parenCount, 0),

            //otherwsise in the normal case bytes get sent to the output
            (_, _, _) => (LspState.Normal, LspResult.ProducedByte, parenCount, input)
        };
    }

    public enum LspResult
    {
        ProducedByte,
        NoByte,
        Done
    }

    public enum LspState
    {
        Normal = 0, // this makes it the default state because structs default to all 0's`
        Escaped,
        InsideEoln,
        Done
    }
}