using System;
using System.Diagnostics;

namespace Melville.Pdf.LowLevel.Filters.CCITTFaxDecodeFilters;

public record struct CcittLineComparison(int A1, int A2, int B1, int B2)
{
    public bool CanVerticalEncode => Math.Abs(VerticalEncodingDelta) <= 4;
    public int VerticalEncodingDelta => A1 - B1;
}

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

    public CcittLinePair SwapLines() => new CcittLinePair(CurrentLine, PriorLine);

    public CcittLineComparison CompareLinesFrom(int a0)
    {
        bool baseColor = ImputedColorAt(CurrentLine, a0);
        var a1 = FindColor(CurrentLine, a0+1, !baseColor);
        var a2 = FindColor(CurrentLine, a1, baseColor);
        var b1 = ImputedColorAt(PriorLine, a0) == baseColor ?
            FindColor(PriorLine, a0+1, !baseColor):
            FindColor(PriorLine, FindColor(PriorLine, a0+1, baseColor) , !baseColor);
        var b2 = FindColor(PriorLine, b1, baseColor);
        return new CcittLineComparison(a1, a2, b1, b2);
    }

    private bool ImputedColorAt(bool[] line, int position) => position < 0 ? true : line[position];

    private static int FindColor(bool[] line, int position, bool color)
    {
        while (position < line.Length && line[position] != color) position++;
        return position;
    }
}