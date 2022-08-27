using Melville.Pdf.LowLevel.Filters.Jpeg;
using Xunit;

namespace Melville.Pdf.DataModelTests.Standard.S7_4Filters.S7_4_8DctDecodeFilters;

public class IdctTest
{
    [Theory]
    [InlineData(0, 0, 64)]
    [InlineData(0, 1, 56)]
    [InlineData(0, 2, 56)]
    [InlineData(0, 3, 57)]
    [InlineData(0, 4, 70)]
    [InlineData(0, 5, 84)]
    [InlineData(0, 6, 84)]
    [InlineData(0, 7, 59)]
    
    [InlineData(1, 0, 66)]
    [InlineData(1, 1, 64)]
    [InlineData(1, 2, 35)]
    [InlineData(1, 3, 36)]
    [InlineData(1, 4, 87)]
    [InlineData(1, 5, 45)]
    [InlineData(1, 6, 21)]
    [InlineData(1, 7, 58)]
    
    [InlineData(2, 0, 66)]
    [InlineData(2, 1, 66)]
    [InlineData(2, 2, 66)]
    [InlineData(2, 3, 59)]
    [InlineData(2, 4, 35)]
    [InlineData(2, 5, 87)]
    [InlineData(2, 6, 26)]
    [InlineData(2, 7, 104)]
    
    [InlineData(3, 0, 35)]
    [InlineData(3, 1, 75)]
    [InlineData(3, 2, 76)]
    [InlineData(3, 3, 45)]
    [InlineData(3, 4, 81)]
    [InlineData(3, 5, 37)]
    [InlineData(3, 6, 34)]
    [InlineData(3, 7, 35)]
    
    [InlineData(4, 0, 45)]
    [InlineData(4, 1, 96)]
    [InlineData(4, 2, 125)]
    [InlineData(4, 3, 107)]
    [InlineData(4, 4, 31)]
    [InlineData(4, 5, 15)]
    [InlineData(4, 6, 107)]
    [InlineData(4, 7, 90)]
    
    [InlineData(5, 0, 88)]
    [InlineData(5, 1, 89)]
    [InlineData(5, 2, 88)]
    [InlineData(5, 3, 78)]
    [InlineData(5, 4, 64)]
    [InlineData(5, 5, 57)]
    [InlineData(5, 6, 85)]
    [InlineData(5, 7, 81)]
    
    [InlineData(6, 0, 62)]
    [InlineData(6, 1, 59)]
    [InlineData(6, 2, 68)]
    [InlineData(6, 3, 113)]
    [InlineData(6, 4, 144)]
    [InlineData(6, 5, 104)]
    [InlineData(6, 6, 66)]
    [InlineData(6, 7, 73)]
    
    [InlineData(7, 0, 107)]
    [InlineData(7, 1, 121)]
    [InlineData(7, 2, 89)]
    [InlineData(7, 3, 21)]
    [InlineData(7, 4, 35)]
    [InlineData(7, 5, 64)]
    [InlineData(7, 6, 65)]
    [InlineData(7, 7, 65)]
    public void FromWebExamole(int row, int column, double value)
    { 
        var ksource = new Matrix8x8<double>( new[]
        {
            -477.63, 24.47, 6.93, -25.49, -6.13, -27.83, -0.57, 6.89,
            -65.84, -22.93, -4.66, 15.25, 16.3, -12.69, 12.2, -7.67,
            7.72, -5.29, 14.03, 74.8, 3.88, -15.81, 13.35, -1.86,
            44.54, -25.13, -24.48, -14.24, 3.35, 47.02, -33.93, 13.8,
            -13.63, 22.85, 22.83, -31.1, -53.13, 22, -22.31, 20.27,
            11.12, -32.74, -64.88, 40.32, 17.61, -11.14, 11.72,-2.59,
            10.47, 6.93, 62.85,  -8.64, -30.16, 17.07, 26.22, -22.7,
            42.7, -31.38, -4.03, -35.84, 0.41, 29.19, 10.36, -27.19
        });
        Assert.Equal(value, DiscreteCosineTransformation.GetInverseElement(row, column, ksource), 1);
    }

    [Fact]
    public void AllWhite()
    {
        var data = new double[64];
        data[0] = 1016;
        for (int i = 0; i < 8; i++)
        {
            for (int j = 0; j < 8; j++)
            {
                var inverseElement = DiscreteCosineTransformation.GetInverseElement(i,j,
                    new Matrix8x8<double>(data));
                Assert.Equal(255, inverseElement, 1);
            }
        }
    }
}