using System.Linq;
using Melville.Pdf.LowLevel.Filters.CCITTFaxDecodeFilters;
using Xunit;

namespace Melville.Pdf.DataModelTests.Writer.CcittParts;

public class LineComparisonTest
{
    [Theory]
    [MemberData(nameof(TestCases))]
    public void LineComparisonTestCase(LinePair pair, LineComparison result, int a0)
    {
        Assert.Equal(result, pair.CompareLinesFrom(a0));
    }
    public static object[][] TestCases()
    {
        return new object[][]
        {
            TestCase("WWWWWWWW", "WWWWWWWW", -1,8,8,8,8),
            TestCase("WW", "  ", -1, 0,2, 2,2),
            TestCase("WWW", "W W", -1, 1,2, 3,3),
            TestCase("WWWWWWWWWWW", "WWW     WWW", -1,  3,8, 11, 11),
            TestCase("W WW", "WWWW", -1, 4,4, 1,2),
            TestCase("  WW", "WWWW", -1, 4,4, 0,2),
            TestCase("    ", "WWWW", -1, 4,4, 0,4),
            TestCase("    WWWWW   WWW", // this is the worked example in sec 2.2.2 of Itu Rec T.6
                     "  WWWWW      WW", 2, 7,13, 9,12),
            TestCase("    WW", 
                     "  WWWW", 1,  2,6, 4,6),
        };
    }
    private static object[] TestCase(string prior, string current, int a0, int a1, int a2, int b1, int b2)
    {
        var pair = new LinePair(RowFromStream(prior), RowFromStream(current));
        var result = new LineComparison(a1, a2, b1, b2);
        return new object[] { pair, result, a0 };
    }

    private static bool[] RowFromStream(string prior) => prior.Select(i => i == 'W').ToArray();
}