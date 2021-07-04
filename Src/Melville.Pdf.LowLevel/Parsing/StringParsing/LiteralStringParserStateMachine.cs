using System.Buffers;

namespace Melville.Pdf.LowLevel.Parsing.StringParsing
{
    
    public ref struct LiteralStringParserStateMachine
    {
        private enum State
        {
            Normal = 0, // this makes it the default state because structs default to all 0's`
            Escaped,
            EndOfLine,
            Octal1,
            Octal2,
            Done
        }

        private State state;
        private int parenCount;
        private int octalValue;
        private readonly LiteralStringParserTarget callback;

        public LiteralStringParserStateMachine(LiteralStringParserTarget callback)
        {
            octalValue = 0;
            state = State.Normal;
            parenCount = 0;
            this.callback = callback;
        }

        public bool TryParse(ref SequenceReader<byte> input)
        {
            while (state != State.Done)
            {
                if (!input.TryRead(out var character)) return false;
                ProcessSingleByte(character);
            }
            return true;
        }

        private void ProcessSingleByte(byte input) => state = Transition(state, input);

        private State Transition(State localState, byte input) => (localState, (char)input) switch
            {
                // count parentheses and kill the process when we get back to zero
                (State.Normal, '(') when parenCount == 0 => ModifyParens(1, State.Normal),
                (State.Normal, ')') when parenCount == 1 => State.Done,
            
                //Internal balanced parens get reflected in the output
                (State.Normal, '(') => ModifyParens(1, WithValue(input, State.Normal)),
                (State.Normal, ')') => ModifyParens(-1, WithValue(input, State.Normal)),
           
                //ReverseSolidus transitions to escape mode
                (State.Normal, '\\') => State.Escaped,
                (State.Escaped, 'n') => WithValue('\n', State.Normal),
                (State.Escaped, 'r') => WithValue('\r', State.Normal),
                (State.Escaped, 't') => WithValue('\t', State.Normal),
                (State.Escaped, 'b') => WithValue('\b', State.Normal),
                (State.Escaped, 'f') => WithValue('\f', State.Normal),
                  // notice that \( |) and \\ are handled by the general clause below
                  //because the relevant productions above require State.Normal for their 
                  // special behaviors for those three characters
            
                //End of line 
                (State.Normal, '\r' or '\n') => WithValue('\n', State.EndOfLine),
                (State.Escaped, '\r' or '\n') => State.EndOfLine,
                (State.EndOfLine, '\r' or '\n') => State.EndOfLine,
                (State.EndOfLine, _) => Transition(State.Normal, input),
            
                // octal literals
                (State.Escaped, >= '0' and <= '7') => FirstOctal(input, State.Octal1),
                (State.Octal1, >= '0' and <= '7') => AddBitsToOctal(input, State.Octal2),
                (State.Octal2, >= '0' and <= '7') => ReturnOctal(AddBitsToOctal(input, State.Normal)),
                (State.Octal1 or State.Octal2, _) => Transition(ReturnOctal(State.Normal), input),
            
                //otherwsise in the normal case bytes get sent to the output
                (_, _) => WithValue(input, State.Normal)
            };

        private State ModifyParens(int delta, State nextState)
        {
            parenCount += delta;
            return nextState;
        }

        private State AddBitsToOctal(byte value, State nextState)
        {
            octalValue <<= 3;
            octalValue |= value - (byte)'0';
            return nextState;
        }

        private State FirstOctal(byte value, State nextState)
        {
            octalValue = value - (byte)'0';
            return nextState;
        }

        private State ReturnOctal(State next) => WithValue((byte) octalValue, next);

        private State WithValue(char value, State nextState) =>
            WithValue((byte) value, nextState);
        private State WithValue(byte value, State nextState)
        {
            callback.AddByte(value);
            return nextState;
        }
    }
}