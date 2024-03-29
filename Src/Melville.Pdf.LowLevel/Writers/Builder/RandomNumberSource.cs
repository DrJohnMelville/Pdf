﻿using System;
using System.Security.Cryptography;
using Melville.INPC;

namespace Melville.Pdf.LowLevel.Writers.Builder;

internal interface IRandomNumberSource
{
    void Fill(in Span<byte> bytes);
}

[StaticSingleton()]
internal partial class RandomNumberSource: IRandomNumberSource
{
    public void Fill(in Span<byte> bytes) => RandomNumberGenerator.Fill(bytes);
}