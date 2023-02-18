using System;
using System.Diagnostics;
using System.Numerics;

namespace Melville.Pdf.Model.Renderers.Patterns.ShaderPatterns;

internal readonly struct RadialShadingComputer
{
    private readonly double x0, y0;
    public RadialShadingComputer(params double[] coords) : this()
    {
        Debug.Assert(coords.Length == 6);
        x0 = coords[0];
        y0 = coords[1];
        var r0 = coords[2];
        var x1 = coords[3];
        var y1 = coords[4];
        var r1 = coords[5];
        
        xDelta = x1 - x0;
        yDelta = y1 - y0;
        var rDelta = r1 - r0;

        x0Sq = Square(x0);
        y0Sq = Square(y0);
        r0Sq = Square(r0);
        rTFactor = 2 * r0 * rDelta;

        A = Square(xDelta) + Square(yDelta) - Square(rDelta);
    }

    private readonly double xDelta, yDelta;
    private readonly double x0Sq, y0Sq, r0Sq;
    private readonly double rTFactor, A;

    public bool TParameterFor(Vector2 patternVal, out double lowRoot, out double highRoot)
    {
        var xPoint = patternVal.X;
        var yPoint = patternVal.Y;

        var xTFactor = 2 * xDelta * (x0 - xPoint);
        var yTFactor = 2 * yDelta * (y0 - yPoint);
        var xConstants = Square(xPoint) + x0Sq - (2*xPoint * x0);
        var yConstants = Square(yPoint) + y0Sq - (2*yPoint * y0);
        
        var B = xTFactor + yTFactor - rTFactor;
        var C = xConstants + yConstants - r0Sq;

        return QuadraticFormula(A, B, C, out lowRoot, out highRoot);
    }

    private bool QuadraticFormula(double A, double B, double C, out double lowRoot, out double highRoot)
    {
        var determinant = Square(B) - (4 * A * C);
        if (determinant < 0 || A == 0)
        {
            highRoot = lowRoot = 0.0;
            return false;
        }

        var root = Math.Sqrt(determinant);
        lowRoot = (-B - root) / (2 * A);
        highRoot = (-B + root) / (2 * A);
        return true;
    }

    private double Square(double x) => x * x;
}