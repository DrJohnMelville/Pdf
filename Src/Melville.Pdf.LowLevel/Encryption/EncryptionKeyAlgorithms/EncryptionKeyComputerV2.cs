using System;
using System.Buffers;
using System.Linq;
using System.Security.Cryptography;
using Melville.Pdf.LowLevel.Encryption.SecurityHandlers;

namespace Melville.Pdf.LowLevel.Encryption.EncryptionKeyAlgorithms;

public interface IGlobalEncryptionKeyComputer
{
    byte[] ComputeKey(string userPassword, in EncryptionParameters parameters);
}
public class GlobalEncryptionKeyComputerV2: IGlobalEncryptionKeyComputer
{
    public byte[] ComputeKey(string userPassword, in EncryptionParameters parameters)
    {
        HashAlgorithm hash = MD5.Create();
        AddPaddedUserPasswordToHash(userPassword, hash);
        hash.AddData(parameters.OwnerPasswordHash);
        AddLittleEndianInt(hash, parameters.Permissions);
        hash.AddData(parameters.IdFirstElement);
        var bytesInKey = parameters.KeyLengthInBits/8;
        var bits = V3Spin(hash, bytesInKey);
        return (bits.Length == bytesInKey) ? bits : bits[..bytesInKey];
    }

    private static void AddPaddedUserPasswordToHash(string userPassword, HashAlgorithm hash)
    {
        var data = ArrayPool<byte>.Shared.Rent(32);
        BytePadder.Pad(userPassword, data.AsSpan(0, 32));
        hash.AddData(data, 32);
        ArrayPool<byte>.Shared.Return(data);
    }

    protected virtual byte[] V3Spin(HashAlgorithm hash, int bytesInKey)
    {
        hash.FinalizeHash();
        return hash.Hash ?? throw new PdfSecurityException("Should have a hash");
    }

    private static void AddLittleEndianInt(HashAlgorithm hash, uint localP)
    {
        var pValue = ArrayPool<byte>.Shared.Rent(4);
        for (int i = 0; i < 4; i++)
        {
            pValue[i] = (byte)localP;
            localP >>= 8;
        }
        hash.AddData(pValue, 4);
        ArrayPool<byte>.Shared.Return(pValue);
    }
}

public class GlobalEncryptionKeyComputerV3 : GlobalEncryptionKeyComputerV2
{
    protected override byte[] V3Spin(HashAlgorithm hash, int bytesInKey)
    {
        var ret = base.V3Spin(hash, bytesInKey);
        for (int i = 0; i < 50; i++)
        {
            hash.AddData(ret, bytesInKey);
            hash.FinalizeHash();
            ret = hash.Hash ?? throw new PdfSecurityException("Hash codes should exist");
        }
        return ret;
    }
}