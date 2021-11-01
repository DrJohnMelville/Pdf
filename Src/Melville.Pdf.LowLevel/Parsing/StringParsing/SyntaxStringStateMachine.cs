using System.Buffers;

namespace Melville.Pdf.LowLevel.Parsing.StringParsing;

public enum SyntaxStringResult
{
    ByteProduced,
    EndOfString,
    EndOfInput
}

public ref struct SyntaxStringStateMachine
{
    private enum State
    {
        InitialState = 0, // this makes it the default state because structs default to all 0's`
        Normal,
        Escaped,
        EndOfLine,
        Octal1,
        Octal2,
        Done
    }

    private State state;
    private int parenCount;
    private int octalValue;
    private int nextChar;
    private int returnChar;
        
    public SyntaxStringResult TryOneParse(ref SequenceReader<byte> input, out byte character)
    {
        character = 0;
        returnChar = -1;
        while (state != State.Done)
        {
            if (!SingleCycle(ref input)) return SyntaxStringResult.EndOfInput;
            if (returnChar >= 0)
            {
                character = (byte) returnChar;
                returnChar = -1;
                return SyntaxStringResult.ByteProduced;
            }
        }

        return SyntaxStringResult.EndOfString;
    }

    private bool SingleCycle(ref SequenceReader<byte> input)
    {
        byte character = 0;
        if (nextChar < 0)
        {
            if (!input.TryRead(out character)) return false;
        }
        else
        {
            character = (byte) nextChar;
            nextChar = -1;
        }

        ProcessSingleByte(character);
        return true;
    }

    private void ProcessSingleByte(byte input) => state = Transition(state, input);

    private State Transition(State localState, byte input) => (localState, (char) input) switch
    {
        //initialization
        (State.InitialState, _) => WithNextChar(-1, State.Normal),
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
        (State.EndOfLine, _) => WithNextChar(input, State.Normal),

        // octal literals
        (State.Escaped, >= '0' and <= '7') => FirstOctal(input, State.Octal1),
        (State.Octal1, >= '0' and <= '7') => AddBitsToOctal(input, State.Octal2),
        (State.Octal2, >= '0' and <= '7') => ReturnOctal(AddBitsToOctal(input, State.Normal)),
        (State.Octal1 or State.Octal2, _) => WithNextChar(input, ReturnOctal(State.Normal)),

        //otherwsise in the normal case bytes get sent to the output
        (_, _) => WithValue(input, State.Normal)
    };


    private State WithNextChar(int next, State nextstate)
    {
        nextChar = next;
        return nextstate;
    }

    private State ModifyParens(int delta, State nextState)
    {
        parenCount += delta;
        return nextState;
    }

    private State AddBitsToOctal(byte value, State nextState)
    {
        octalValue <<= 3;
        octalValue |= value - (byte) '0';
        return nextState;
    }

    private State FirstOctal(byte value, State nextState)
    {
        octalValue = value - (byte) '0';
        return nextState;
    }

    private State ReturnOctal(State next) => WithValue((byte) octalValue, next);

    private State WithValue(char value, State nextState) =>
        WithValue((byte) value, nextState);

    private State WithValue(byte value, State nextState)
    {
        returnChar = value;
        return nextState;
    }
}