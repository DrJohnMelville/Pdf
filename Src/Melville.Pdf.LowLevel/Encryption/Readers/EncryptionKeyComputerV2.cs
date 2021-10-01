using System;
using System.Buffers;
using System.Security.Cryptography;

namespace Melville.Pdf.LowLevel.Encryption.Readers
{
    public interface IEncryptionKeyComputer
    {
        byte[] ComputeKey(in ReadOnlySpan<byte> userPassword, in EncryptionParameters parameters);
    }
    public class EncryptionKeyComputerV2: IEncryptionKeyComputer
    {
        public byte[] ComputeKey(in ReadOnlySpan<byte> userPassword, in EncryptionParameters parameters)
        {

            HashAlgorithm hash = MD5.Create();
            hash.AddData(BytePadder.Pad(userPassword));
            hash.AddData(parameters.OwnerPasswordHash);
            AddLittleEndianInt(hash, parameters.Permissions);
            hash.AddData(parameters.IdFirstElement);
            var bytesInKey = parameters.KeyLengthInBits/8;
            var bits = V3Spin(hash, bytesInKey);
            return (bits.Length == bytesInKey) ? bits : bits[..bytesInKey];
        }

        protected virtual byte[] V3Spin(HashAlgorithm hash, int bytesInKey)
        {
            hash.FinalizeHash();
            return hash.Hash ?? throw new InvalidProgramException("Should have a hash");
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

    public class EncryptionKeyComputerV3 : EncryptionKeyComputerV2
    {
        protected override byte[] V3Spin(HashAlgorithm hash, int bytesInKey)
        {
            var ret = base.V3Spin(hash, bytesInKey);
            for (int i = 0; i < 50; i++)
            {
                hash.AddData(ret, bytesInKey);
                hash.FinalizeHash();
                ret = hash.Hash ?? throw new InvalidProgramException("Hash codes should exist");
            }
            return ret;
        }
    }
}