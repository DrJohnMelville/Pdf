using System;
using Melville.Pdf.LowLevel.Writers.Builder;
using Xunit;

namespace Melville.Pdf.DataModelTests.Standard.S7_6Encryption.S7_6_3_4PasswordAlgorithms.V6Algorithms;

public class Modulo3Test
{
    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    [InlineData(int.MaxValue)]
    [InlineData(int.MaxValue-1)]
    [InlineData(int.MaxValue-2)]
    [InlineData(int.MaxValue-3)]
    [InlineData(int.MaxValue/2)]
    public void Modulo3(int value)
    {
        var answer = value % 3;
        Span<byte> bytes = stackalloc byte[]
        {
            (byte)(value >> 24),
            (byte)(value >> 16),
            (byte)(value >> 08),
            (byte)(value),
        };
        Assert.Equal(answer, ((ReadOnlySpan<byte>)bytes).Mod3());
        
        
    }
}