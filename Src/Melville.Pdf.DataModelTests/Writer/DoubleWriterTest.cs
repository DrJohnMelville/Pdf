using System;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Writers.ObjectWriters;

namespace Melville.Pdf.DataModelTests.Writer;

public class DoubleWriterTest
{
    [Test]
    [Arguments(1, "1",17)]
    [Arguments(2, "2", 17)]
    [Arguments(2.4,"2.4", 17)]
    [Arguments(2.45,"2.45", 17)]
    [Arguments(-2.4,"-2.4", 17)]
    [Arguments(-0.5,"-0.5", 17)]
    [Arguments(Math.PI, "3.141592653589793", 17)]
    [Arguments(Math.PI*100, "314.1592653589793", 17)]
    [Arguments(2.9999999, "3", 5)]
    [Arguments(69.9999999, "70", 5)]
    [Arguments(99.9999999, "100", 5)]
    [Arguments(-99.9999999, "-100", 5)]
    public void WriteDouble(double d, string result, int length)
    {
        Span<byte> ret = stackalloc byte[length];
        var len = DoubleWriter.Write(d, ret);
        Assert.Equal(result, ExtendedAsciiEncoding.ExtendedAsciiString(ret[..len]));
    }
}