using System;
using Xunit;

namespace Melville.ArchitectureAnalyzer.Test.InternalRuleTest;

public static class PositionOfImplementation
{
    public static (int line, int col) PositionOf(this in ReadOnlySpan<char> source, in ReadOnlySpan<char> target)
    {
        var index = source.IndexOf(target);
        return index < 0? (-1,-1): source[0..index].LinesAndCols();
    }

    public static (int line, int col) LinesAndCols(this in ReadOnlySpan<char> source)
    {
        int line = 1;
        int col = 1;
        foreach (var character in source)
        {
            if (character == '\n')
            {
                line++;
                col = 1;
            }
            else
            {
                col++;
            }
        }
        return (line, col);
    }
}

public class PositionOfTest
{
    [Theory]
    [InlineData("AAA", 1, 1)]
    [InlineData("     AAA", 1, 6)]
    [InlineData("\n     AAA", 2, 6)]
    [InlineData("jkhdwskihfg'hkfbdhkfd\n     AAA", 2, 6)]
    [InlineData("jkhdwskihfg'hkfbdhkfd\n12345AAA", 2, 6)]
    [InlineData("jkhdwskih\r\nfg'hkfbdhkfd\n12345AAA", 3, 6)]
    public void TestLinesAndCol(string input, int line, int col)
    {
        Assert.Equal((line, col), input.AsSpan().PositionOf("AAA"));
    }

}
