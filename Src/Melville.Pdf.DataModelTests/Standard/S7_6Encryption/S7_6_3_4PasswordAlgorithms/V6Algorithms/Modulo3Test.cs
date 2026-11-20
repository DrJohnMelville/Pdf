using System;
using Melville.Pdf.LowLevel.Writers.Builder.EncryptionV6;

namespace Melville.Pdf.DataModelTests.Standard.S7_6Encryption.S7_6_3_4PasswordAlgorithms.V6Algorithms;

public class Modulo3Test
{
    [Test]
    [Arguments(0)]
    [Arguments(1)]
    [Arguments(2)]
    [Arguments(3)]
    [Arguments(int.MaxValue)]
    [Arguments(int.MaxValue-1)]
    [Arguments(int.MaxValue-2)]
    [Arguments(int.MaxValue-3)]
    [Arguments(int.MaxValue/2)]
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