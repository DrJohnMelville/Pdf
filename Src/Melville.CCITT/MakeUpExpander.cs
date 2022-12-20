namespace Melville.CCITT;

internal class MakeUpExpander : ICodeDictionay
{
    private readonly ICodeDictionay innerDictionary;
    private int extraPixels = 0;

    public MakeUpExpander(ICodeDictionay innerDictionary)
    {
        this.innerDictionary = innerDictionary;
    }

    public bool TryReadCode(in (int BitLength, int SourceBits) input, bool isWhiteRun, out CcittCode code)
    {
        if (!innerDictionary.TryReadCode(input, isWhiteRun, out code)) return false;
        switch (code.Operation)
        {
            case CcittCodeOperation.HorizontalBlack:
            case CcittCodeOperation.HorizontalWhite:
                AddSavedPixelsToRun(ref code);
                break;
            case CcittCodeOperation.MakeUp:
                RememberSavedPixels(ref code);
                break;
        }
        return true;
    }

    private void AddSavedPixelsToRun(ref CcittCode code)
    {
        code = code with { Length = (ushort)(code.Length + extraPixels) };
        extraPixels = 0;
    }

    private void RememberSavedPixels(ref CcittCode code)
    {
        extraPixels += code.Length;
        code = new CcittCode(CcittCodeOperation.NoCode, 0);
    }

    public bool IsAtValidEndOfLine => extraPixels == 0;
}