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
        var baseColor = ImputedColorAt(CurrentLine, a0);
        var a1 = FindColor(CurrentLine, a0+1, !baseColor);
        var a2 = FindColor(CurrentLine, a1, baseColor);
        var b1 = ComputeB1(a0, baseColor);
        var b2 = FindColor(PriorLine, b1, baseColor);
        return new CcittLineComparison(a1, a2, b1, b2);
    }
    
    public int ComputeB2(int a0, bool baseColor) => 
        FindColor(PriorLine, ComputeB1(a0, baseColor), baseColor);

    public int ComputeB1(int a0, bool baseColor) => a0 == -1
            ? FindColor(PriorLine, 0, false)
            : FindChangeToOppositeColor(a0, baseColor);

    private int FindChangeToOppositeColor(int a0, bool baseColor)
    {
        Debug.Assert(a0 >= 0);
        return FindColor(PriorLine, FindColor(PriorLine, a0, baseColor), !baseColor);
    }

    private bool ImputedColorAt(bool[] line, int position) => position < 0 || line[position];
    public bool ImputedCurrentColor(int a0) => ImputedColorAt(CurrentLine, a0);

    private static int FindColor(bool[] line, int position, bool color)
    {
        while (position < line.Length && line[position] != color) position++;
        return position;
    }
}