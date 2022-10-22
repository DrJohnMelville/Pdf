using System;
using System.Security.Cryptography;
using Melville.INPC;

namespace Melville.Pdf.LowLevel.Writers.Builder;

public interface IRandomNumberSource
{
    void Fill(in Span<byte> bytes);
}

[StaticSingleton()]
public partial class RandomNumberSource: IRandomNumberSource
{
    public void Fill(in Span<byte> bytes) => RandomNumberGenerator.Fill(bytes);
}