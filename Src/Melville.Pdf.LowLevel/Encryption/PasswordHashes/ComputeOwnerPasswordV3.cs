﻿using System;
using System.Security.Cryptography;
using Melville.INPC;

namespace Melville.Pdf.LowLevel.Encryption.PasswordHashes;

[StaticSingleton()]
internal partial class ComputeOwnerPasswordV3 : ComputeOwnerPasswordV2
{
    protected override int SequentialEncryptionCount() => 20;

    protected override byte[] ComputeMd5Hash(Span<byte> paddedPassword)
    {
        var source = new byte[16];
        Span<byte> dest = stackalloc byte[16];
        MD5.HashData(paddedPassword, source);
        for (int i = 0; i < 25; i++) // unroll the loop by two to avoid swapping the arguments
        {
            MD5.HashData(source, dest);
            MD5.HashData(dest, source);
        }

        return source;
    }
}