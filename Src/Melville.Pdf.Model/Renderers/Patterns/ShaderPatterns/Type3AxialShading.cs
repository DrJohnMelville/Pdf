using System;
using System.Diagnostics;
using System.Numerics;
using Melville.Pdf.LowLevel.Model.Wrappers.Functions;

namespace Melville.Pdf.Model.Renderers.Patterns.ShaderPatterns;

public class Type3AxialShading : ParametricFunctionalShader
{
    private AxialShadingComputer computer;

    public Type3AxialShading(CommonShaderValues common,
        double[] coords, ClosedInterval domain, IPdfFunction function, bool extendLow, bool extendHigh) :
        base(common, domain, function, extendLow, extendHigh)

    {
        computer = new AxialShadingComputer(coords);
    }

    protected override bool TParameterFor(Vector2 patternVal, out double tParameter) =>
        computer.TParameterFor(patternVal, out tParameter);
}

public readonly struct AxialShadingComputer
{
    private readonly double x0, y0;

    public AxialShadingComputer(params double[] coords) : this()
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

    public bool TParameterFor(Vector2 patternVal, out double tParameter)
    {
        var xPoint = patternVal.X;
        var yPoint = patternVal.Y;

        var xTFactor = 2 * xDelta * (x0 - xPoint);
        var yTFactor = 2 * yDelta * (y0 - yPoint);
        var xConstants = Square(xPoint) + x0Sq - (2*xPoint * x0);
        var yConstants = Square(yPoint) + y0Sq - (2*yPoint * y0);
        
        var B = xTFactor + yTFactor - rTFactor;
        var C = xConstants + yConstants - r0Sq;

        if (!QuadraticFormula(A, B, C, out var lowRoot, out var highRoot))
        {
            tParameter = -1;
            return false;
        }

        tParameter = SelectBiggestValidRoot(lowRoot, highRoot);
        return true;
    }

    private static double SelectBiggestValidRoot(double lowRoot, double highRoot) =>
        (highRoot > 1 && lowRoot is >= 0 and <= 1) ? lowRoot : highRoot;
     
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

/*
Derivation of the formula above

xPoint = x coordinate to check
yPoint = y coordinate to check

[xyr]0 = coordinates and radius of first circle
[xyr]1 = coordinates and radius of second circle

Compute
t = the T coordinate for the larger circle containing (xp,yp)

define
xDelta = x1 - x0;
yDelta = y1 - y0;

xi = x center of the shading circle = x0 + (t * xDelta)
yi = y center of the shading circle = y0 + (t * yDelta)
ri = radius of the shading circle = r0 + (t * rDelta)

We know the (xPoint, yPoint) is on circle i therefroe

Square(xPoint - xi) + Square(yPoint - yi) = Square(ri)

Substituting
Square(xPoint - x0 - (t*xDelta)) + Square(yPoint - y0 - (t*yDelta) = Square(r0 + t*rDelta)

--- interlude square of a - b- c
(a -b -c) * (a -b -c)
Square(a) - a*b - a*c  -a*b +Square(b) + b*c     - a*c + b*c + Square(c)

Square(a) + square (b) + square(c) - (2*a*b) -(2*a*c) + (2*b*c) 
------
Square(xPoint) + Square(x0) + Square(t*xDelta) - (2*xPoint * x0) - (2*xPoint*t*xDelta) +(2*x0*T*xDelta) + 
Square( yPoint) + Square(y0) + Square(t*yDelta) - (2*yPoint * y0) - (2*yPoint*t*yDelta) +(2*y0*T*yDelta) =
Square(r0) + 2*r0*T*rDelta + Square(t*rDelta) 

Square(xPoint) + Square(x0) + Square(t)*Square(xDelta) - (2*xPoint * x0) - (2*xPoint*t*xDelta) +(2*x0*T*xDelta) + 
Square( yPoint) + Square(y0) + Square(t)*Square(yDelta) - (2*yPoint * y0) - (2*yPoint*t*yDelta) +(2*y0*T*yDelta) =
Square(r0) + (2*r0*T*rDelta) + (Square(t)*Square(rDelta)) 

Square(t)*Square(xDelta) - (T*2*xPoint*xDelta) +(T*2*x0*xDelta) + Square(xPoint) + Square(x0) - (2*xPoint * x0) + 
Square(t)*Square(yDelta) - (T*2*yPoint*yDelta) +(T*2*y0*yDelta) + Square(yPoint) + Square(y0) - (2*yPoint * y0)  =
Square(t)*Square(rDelta) + t*2*r0*rDelta + Square(rDelta) 

Square(t)*Square(xDelta) - (T* ((2*xPoint*xDelta) +(2*x0*xDelta))) + (Square(xPoint) + Square(x0) - (2*xPoint * x0)) + 
Square(t)*Square(yDelta) - (T* ((2*yPoint*yDelta) +(2*y0*yDelta))) + (Square(yPoint) + Square(y0) - (2*yPoint * y0))  =
Square(t)*Square(rDelta) + t*2*r0*rDelta + Square(rDelta) 

Square(t)*Square(xDelta)  + T *(2*x0*xDelta))) - T* ((2*xPoint*xDelta) + (Square(xPoint) + Square(x0) - (2*xPoint * x0)) + 
Square(t)*Square(yDelta)  + T *(2*y0*yDelta))) - T* ((2*yPoint*yDelta) + (Square(yPoint) + Square(y0) - (2*yPoint * y0))  =
Square(t)*Square(rDelta) + t*2*r0*rDelta + Square(rDelta) 

Square(t)*Square(xDelta)  + T *((2*x0*xDelta))) - ((2*xPoint*xDelta)) + (Square(xPoint) + Square(x0) - (2*xPoint * x0)) + 
Square(t)*Square(yDelta)  + T *((2*y0*yDelta))) - ((2*yPoint*yDelta)) + (Square(yPoint) + Square(y0) - (2*yPoint * y0))  =
Square(t)*Square(rDelta) + t*2*r0*rDelta + Square(rDelta) 


define:
  [xyr]DeltaSq = Square([xyr]Delta)
  [xy]TFactor = 2 * [xy]Delta * ([xy]0 - [xy]Point)
  [xy]Constants = Square([xy]Point) + Square([xy]0) - (2*[xy]Point * [xy]0)
  rTFactor = 2*r0*rDelta

substituting
Square(t)*Square(xDelta) +  T * xTFactor + XConstants +
Square(t)*Square(yDelta) +  T * yTFactor + yConstants +
-Square(t)*Square(rDelta) - T * rTFactor - Square(r0)

Define:
    A = xDeltaSq + yDeltaSq - rDeltaSq
    B = xTFactor + yTFactor - rTFactor
    C = xConstants+yConstants - Square(r0)

Square(T) * A + T * B + C = 0

using the quadratic formula

T =  (-B [+-] Sqrt(Square(B) - (4*A*C))) / (2*a)

because I want the bigger value of T I always use the plus above.

*/