using System;
using System.Buffers;
using System.Diagnostics;
using System.Linq;
using System.Security.Cryptography;
using Melville.Pdf.LowLevel.Encryption.SecurityHandlers;

namespace Melville.Pdf.LowLevel.Encryption.EncryptionKeyAlgorithms;

internal interface IGlobalEncryptionKeyComputer
{
    byte[] ComputeKey(string userPassword, in EncryptionParameters parameters);
}

internal class GlobalEncryptionKeyComputerV2: IGlobalEncryptionKeyComputer
{
    public byte[] ComputeKey(string userPassword, in EncryptionParameters parameters)
    {
        var hash = IncrementalHash.CreateHash(HashAlgorithmName.MD5);
        AddPaddedUserPasswordToHash(userPassword, hash);
        hash.AppendData(parameters.OwnerPasswordHash.Span);
        AddLittleEndianInt(hash, parameters.Permissions);
        hash.AppendData(parameters.IdFirstElement.Span);
        var bytesInKey = parameters.KeyLengthInBits/8;
        var bits = V3Spin(hash, bytesInKey);
        return (bits.Length == bytesInKey) ? bits : bits[..bytesInKey];
    }
    
    private static void AddPaddedUserPasswordToHash(string userPassword, IncrementalHash hash)
    {
        Span<byte> data = stackalloc byte[32];
        BytePadder.Pad(userPassword, data);
        hash.AppendData(data);
    }

    protected virtual byte[] V3Spin(IncrementalHash hash, int bytesInKey)
    {
        return hash.GetHashAndReset() ?? throw new PdfSecurityException("Should have a hash");
    }

    private static void AddLittleEndianInt(IncrementalHash hash, uint localP)
    {
        Span<byte> pValue = stackalloc byte[4];
        for (int i = 0; i < 4; i++)
        {
            pValue[i] = (byte)localP;
            localP >>= 8;
        }
        hash.AppendData(pValue);
    }
}

internal class GlobalEncryptionKeyComputerV3 : GlobalEncryptionKeyComputerV2
{
    protected override byte[] V3Spin(IncrementalHash hash, int bytesInKey)
    {
        var ret = base.V3Spin(hash, bytesInKey);
        for (int i = 0; i < 50; i++)
        {
            hash.AppendData(ret[..bytesInKey]);
            ret = base.V3Spin(hash, bytesInKey);
        }
        return ret;
    }
}