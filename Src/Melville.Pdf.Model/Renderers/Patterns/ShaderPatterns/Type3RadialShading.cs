using System.Collections.Generic;
using System.Numerics;
using Melville.Pdf.LowLevel.Model.Wrappers.Functions;

namespace Melville.Pdf.Model.Renderers.Patterns.ShaderPatterns;

internal sealed class Type3RadialShading : ParametricFunctionalShader
{
    private RadialShadingComputer computer;

    public Type3RadialShading(CommonShaderValues common,
        IReadOnlyList<double> coords, ClosedInterval domain, IPdfFunction function, bool extendLow, bool extendHigh) :
        base(common, domain, function, extendLow, extendHigh)
    {
        computer = new RadialShadingComputer(coords);
    }

    protected override uint GetColorFromShader(Vector2 patternVal)
    {
        if (!computer.TParameterFor(patternVal, out double lowRoot, out double highRoot))
            return BackgroundColor; // object not hit.
        var (highCol, isBackground) = ColorFromT(highRoot);
        var ret = isBackground? ColorFromT(lowRoot).Color : highCol;
        return  ret;
    }
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