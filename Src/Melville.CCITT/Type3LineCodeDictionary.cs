namespace Melville.CCITT;

internal abstract class Type3LineCodeDictionary : ICodeDictionay
{
    
    public bool TryReadCode(in (int BitLength, int SourceBits) input, bool isWhiteRun, out CcittCode code)
    {
        if (IsEndOfLineCode(input)) return SetEndOfLineState(out code);
        IsAtValidEndOfLine = false;
        return LineReader.TryReadCode(input, isWhiteRun, out code);
    }

    private bool SetEndOfLineState(out CcittCode code)
    {
        IsAtValidEndOfLine = true;
        code = new CcittCode(CcittCodeOperation.EndOfLine, 0);
        return true;
    }
    public bool IsAtValidEndOfLine { get; private set; } = false;
    protected abstract bool IsEndOfLineCode((int BitLength, int SourceBits) input);
    protected abstract ICodeDictionay LineReader { get; }
}

/// <summary>
/// This type implements Group 3, 1-D from the ITU Rec T.4
/// </summary>
internal class Type3K0LineCodeDictionary : Type3LineCodeDictionary
{
    protected override bool IsEndOfLineCode((int BitLength, int SourceBits) input) => 
        input == (12, 0b000000000001);

    protected override ICodeDictionay LineReader { get; } =
        new MakeUpExpander(TerminalCodeDictionary.Instance);
}

/// <summary>
/// This type implements Group 3 2-D from the ITU Rec T.4
/// </summary>
internal class Type3SwitchingLineCodeDictionary: Type3LineCodeDictionary
{
    private readonly ICodeDictionay twoDimensional;
    private readonly ICodeDictionay oneDimensional = new MakeUpExpander(TerminalCodeDictionary.Instance);
    protected override ICodeDictionay LineReader => lineReader;
    private ICodeDictionay lineReader;

    public Type3SwitchingLineCodeDictionary()
    {
        lineReader = oneDimensional;
        twoDimensional = new TwoDimensionalLineCodeDictionary(oneDimensional);
    }
    
    protected override bool IsEndOfLineCode((int BitLength, int SourceBits) input)
    {
        switch (input)
        {
            case (13, 0b0000000000011):
                lineReader = oneDimensional;
                return true;
            case (13, 0b0000000000010):
                lineReader = twoDimensional;
                return true;
        }

        return false;
    }

}
