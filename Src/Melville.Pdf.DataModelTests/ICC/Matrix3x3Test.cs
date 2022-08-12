using Melville.Icc.Model.Tags;
using Xunit;

namespace Melville.Pdf.DataModelTests.ICC;

public class Matrix3x3Test
{
    [Fact]
    public void Add()
    {
        var ret = new Matrix3x3(1, 5, 6, 7, 9, 1, 4, 2, 3) +
                  new Matrix3x3(3, 7, 1, 8, 6, 7, 6, 1, 9);
        Assert.Equal(new Matrix3x3(4, 12, 7, 15, 15, 8, 10, 3, 12),
            ret);

    }
    [Fact]
    public void Times()
    {
        var ret = new Matrix3x3(
                      1, 5, 6,
                      7, 9, 1, 
                      4, 2, 3) *
                  new Matrix3x3(
                      3, 7, 1, 
                      8, 6, 7, 
                      6, 1, 9);
        Assert.Equal(new Matrix3x3(79, 43, 90, 99, 104, 79, 46, 43, 45),
            ret);

    }

    [Fact]
    public void Determinant()
    {
        var mat = new Matrix3x3(-2, -1, 2, 2, 1, 4, -3, 3, -1);
        Assert.Equal(54, mat.Determinant());
        Assert.True(mat.HasInverse());
    }

    [Fact]
    public void CannotInvertZeroMatrix()
    {
        Assert.False(new Matrix3x3(new double[9]).HasInverse());
    }

    [Fact]
    public void MatrixInverse()
    {
        var mat = new Matrix3x3(-2, -1, 2, 2, 1, 4, -3, 3, -1);
        var inv = mat.Inverse();
        Assert.Equal(-0.2407, inv.M11, 3);
        Assert.Equal(0.09259, inv.M12, 3);
        Assert.Equal(-0.1111, inv.M13, 3);
        Assert.Equal(-0.185185, inv.M21, 3);
        Assert.Equal(0.148148, inv.M22, 3);
        Assert.Equal(0.222222, inv.M23, 3);
        Assert.Equal(0.16666, inv.M31, 3);
        Assert.Equal(0.16666, inv.M32, 3);
        Assert.Equal(0.0, inv.M33, 3);
        
    }
}