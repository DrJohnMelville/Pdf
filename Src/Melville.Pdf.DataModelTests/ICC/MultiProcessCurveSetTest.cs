using System;
using System.Buffers;
using System.Threading.Tasks;
using Melville.Icc.Model.Tags;
using Xunit;

namespace Melville.Pdf.DataModelTests.ICC;

public class MultiProcessCurveSetTest
{
    public SequenceReader<byte> FloatToBytes(params float[] values)
    {
        var ret = new byte[values.Length * 4];
        for (int i = 0; i < values.Length; i++)
        {
            var intVal = BitConverter.SingleToUInt32Bits(values[i]);
            for (int j = 3; j >= 0; j--)
            {
                ret[(i * 4) + j] = (byte)intVal;
                intVal >>= 8;
            }
        }
        return new(new ReadOnlySequence<byte>(ret));
    }

    [Fact]
    public void IdentityCurve0()
    {
        var reader = FloatToBytes(1, 1, 0, 0);
        var func = new FormulaSegmentType0(ref reader);
        Assert.Equal(12, func.Evaluate(12));
        Assert.Equal(120, func.Evaluate(120));
    }
    [Fact]
    public void FullCurve0()
    {
        var reader = FloatToBytes(2,2,2,2);
        var func = new FormulaSegmentType0(ref reader);
        Assert.Equal(18, func.Evaluate(1));
    }
    
    [Fact]
    public void FullCurve1()
    {
        var reader = FloatToBytes(2,2,2,2, 2);
        var func = new FormulaSegmentType1(ref reader);
        Assert.Equal(5.432007, func.Evaluate(5), 4);
    }

    [Fact]
    public void FullCurve2()
    {
        var reader = FloatToBytes(2,3,4,5,6);
        var func = new FormulaSegmentType2(ref reader);
        Assert.Equal(1694577197056, func.Evaluate(5), 4);
    }

    [Fact]
    public void LinearInterpolation()
    {
        var segment = new SampledCurveSegment(0, 100);
        segment.Initialize(1,11, 0);

        Assert.Equal(55, segment.Evaluate(6.5f));
        Assert.Equal(70, segment.Evaluate(8f));
        
    }
    [Fact]
    public void Triangle()
    {
        var segment = new SampledCurveSegment(0, 20, 10);
        segment.Initialize(0, 2, 10);

        Assert.Equal(12.5, segment.Evaluate(0.25f));
        Assert.Equal(15, segment.Evaluate(0.5f));
        Assert.Equal(15, segment.Evaluate(1.5f));
        Assert.Equal(12.5, segment.Evaluate(1.75f));
        
    }
    
}