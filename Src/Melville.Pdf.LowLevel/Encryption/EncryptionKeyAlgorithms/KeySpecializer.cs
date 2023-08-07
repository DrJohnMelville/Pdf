using System;
using System.Diagnostics;
using System.Security.Cryptography;
using Melville.INPC;

namespace Melville.Pdf.LowLevel.Encryption.EncryptionKeyAlgorithms;

internal interface IKeySpecializer
{
    ReadOnlySpan<byte> ComputeKeyForObject(byte[] rootKey, int objectNumber, int generationNumber);
}

internal class Rc4KeySpecializer: IKeySpecializer
{
    public static Rc4KeySpecializer Instance = new();
    protected Rc4KeySpecializer(){}
    public ReadOnlySpan<byte> ComputeKeyForObject(byte[] rootKey, int objectNumber, int generationNumber)
    {
        Debug.Assert(objectNumber > 0); // test for a former buggy implementation
        var span = ComputeHash(rootKey, objectNumber, generationNumber).AsSpan();
        return span[..EncryptionKeyLength(rootKey.Length)];
    }

    private byte[] ComputeHash(byte[] rootKey, int objectNumber, int generationNumber)
    {
        var md5 = IncrementalHash.CreateHash(HashAlgorithmName.MD5);
        md5.AppendData(rootKey);
        AddObjectData(objectNumber, generationNumber, md5);
        return md5.GetCurrentHash() ?? throw new InvalidProgramException("Should have a hash here.");
    }

    protected virtual void AddObjectData(int objectNumber, int generationNumber, IncrementalHash md5) =>
        md5.AppendData(stackalloc []
        {
            (byte)objectNumber,
            (byte)(objectNumber >> 8),
            (byte)(objectNumber >> 16),
            (byte)generationNumber,
            (byte)(generationNumber >> 8),
        });

    private static int EncryptionKeyLength(int baseKeyLength) => Math.Min(baseKeyLength + 5, 16);
}

[StaticSingleton("AesInstance")]
internal partial class AesKeySpecializer: Rc4KeySpecializer
{
    private static readonly byte[] aesSalt = {0x73,0x41,0x6c,0x54 };
    protected override void AddObjectData(int objectNumber, int generationNumber, IncrementalHash md5)
    {
        base.AddObjectData(objectNumber, generationNumber, md5);
        md5.AppendData(aesSalt);
    }
}

[StaticSingleton]
internal partial class AesV6KeySpecializer : IKeySpecializer
{
    public ReadOnlySpan<byte> ComputeKeyForObject(byte[] rootKey, int objectNumber, int generationNumber)
    {
        return rootKey;
    }
}