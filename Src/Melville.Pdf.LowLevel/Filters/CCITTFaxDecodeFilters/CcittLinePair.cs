using System.Diagnostics;

namespace Melville.Pdf.LowLevel.Filters.CCITTFaxDecodeFilters;

public readonly struct CcittLinePair
{
    public bool[] PriorLine { get; }
    public bool[] CurrentLine { get; }
    public int LineLength => CurrentLine.Length;

    public CcittLinePair(bool[] priorLine, bool[] currentLine)
    {
        Debug.Assert(priorLine.Length == currentLine.Length);
        PriorLine = priorLine;
        CurrentLine = currentLine;
    }
    
    public CcittLinePair(in CcittParameters parameters): 
        this(parameters.CreateWhiteRow(), parameters.EmptyLine()){}

    public CcittLinePair SwapLines() => new(CurrentLine, PriorLine);

    public CcittLineComparison CompareLinesFrom(int a0)
    {
        bool baseColor = ImputedColorAt(CurrentLine, a0);
        var a1 = FindColor(CurrentLine, a0+1, !baseColor);
        var a2 = FindColor(CurrentLine, a1, baseColor);
        var b1 = ComputeB1(a0, baseColor);
        var b2 = FindColor(PriorLine, b1, baseColor);
        return new CcittLineComparison(a1, a2, b1, b2);
    }
    
    public int ComputeB2(int a0, bool baseColor)
    {
        var b1 = ComputeB1(a0, baseColor);
        return FindColor(PriorLine, b1, baseColor);
    }

    public int ComputeB1(int a0, bool baseColor)
    {
        Debug.Assert(B1AlgA(a0, baseColor) == B1AlgB(a0, baseColor));
        return B1AlgA(a0, baseColor);
    }

    private int B1AlgA(int a0, bool baseColor) => 
        FindImputedColor(PriorLine, FindImputedColor(PriorLine, a0, baseColor), !baseColor);

    private int B1AlgB(int a0, bool baseColor)
    {
        int ret;
        for (ret = a0 + 1; ret < LineLength && IsNotPointB1(baseColor, ret); ret++) ; // empty loop body
        return ret;
    }
    private bool IsNotPointB1(bool baseColor, int ret) =>
        SameAsBaseColor(baseColor, ret) || IsNotChangingPixel(ret);
    private bool IsNotChangingPixel(int ret) => PriorLine[ret] == ImputedColorAt(PriorLine, ret - 1);
    private bool SameAsBaseColor(bool baseColor, int ret) => PriorLine[ret] == baseColor;

    private bool ImputedColorAt(bool[] line, int position) => position < 0 || line[position];
    public bool ImputedCurrentColor(int a0) => ImputedColorAt(CurrentLine, a0);

    private static int FindImputedColor(bool[] line, int position, bool color) => (position, color) switch 
    {
        (-1,true) => -1,
        (-1, false) => FindColor(line, 0, false),
        _=> FindColor(line, position, color)
    };
    
    private static int FindColor(bool[] line, int position, bool color)
    {
        while (position < line.Length && line[position] != color) position++;
        return position;
    }
}