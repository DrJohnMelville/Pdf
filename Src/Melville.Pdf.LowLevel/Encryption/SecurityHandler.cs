using System;
using System.Net;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Melville.Pdf.LowLevel.Encryption.Cryptography;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Model.Primitives;
using Melville.Pdf.LowLevel.Parsing.ObjectParsers;

namespace Melville.Pdf.LowLevel.Encryption
{
    public interface ISecurityHandler
    {
        bool TyyUserPassword(in Span<byte> password);
    }

    public static class SecurityHandlerOperations
    {
        public static bool TryUserPassword(this ISecurityHandler handler, string password) =>
          handler.TyyUserPassword(password.AsExtendedAsciiBytes());
    }

    public static class SecurityHandlerFactory
    {
        public static async ValueTask<ISecurityHandler> CreateSecurityHandler(PdfDictionary trailer)
        {
            if (await trailer.GetOrNullAsync(KnownNames.Encrypt) is not PdfDictionary dict)
                throw new PdfSecurityException("Not encrypted");
                
            if (await dict.GetAsync<PdfName>(KnownNames.Filter) != KnownNames.Standard)
                throw new PdfSecurityException("Only standard security handler is supported.");
        
            var V = (await dict.GetAsync<PdfNumber>(KnownNames.V)).IntValue;
            var R = (await dict.GetAsync<PdfNumber>(KnownNames.R)).IntValue;
            
            return (V,R)switch
            {
                (0 or 3, _) => throw new PdfSecurityException("Undocumented Algorithms are not supported"),
                (4, _) => throw new PdfSecurityException("Default CryptFilters are not supported."),
                (1 or 2, 2) => new SecurityHandlerV2( await EncryptionParameters.Create(trailer)),
                (1 or 2, 3) => new SecurityHandlerV3(await EncryptionParameters.Create(trailer)),
                (_, 4) => throw new PdfSecurityException(
                    "Standard Security handler V4 requires a encryption value of 4  and is unsupported."),
                _ => throw new PdfSecurityException("Unrecognized encryption algorithm (V)")
            };
        }
    }


    public class SecurityHandlerV2 : ISecurityHandler
    {
        protected readonly EncryptionParameters Parameters;

        public SecurityHandlerV2(EncryptionParameters parameters)
        {
            this.Parameters = parameters;
        }

        public bool TyyUserPassword(in Span<byte> password)
        {
            var key = KeyComputer().ComputeKey(password, Parameters);
            return (CompareUserHash(ComputeUserPasswordHash(key), Parameters.UserPasswordHash));
        }

        protected virtual bool CompareUserHash(in ReadOnlySpan<byte> a, in ReadOnlySpan<byte> b) => 
            a.SequenceCompareTo(b) == 0;

        protected virtual byte[] ComputeUserPasswordHash(byte[] encryptionKey)
        {
            var rc4 = new RC4(encryptionKey);
            var ret = new byte[32];
            rc4.Transform(BytePadder.PdfPasswordPaddingBytes, ret);
            return ret;
        }

        protected virtual IEncryptionKeyComputer KeyComputer() =>
            new EncryptionKeyComputerV2();
    }
    
    public class SecurityHandlerV3 : SecurityHandlerV2
    {
        public SecurityHandlerV3(EncryptionParameters parameters) : base(parameters)
        {
        }

        protected override IEncryptionKeyComputer KeyComputer() => new EncryptionKeyComputerV3();
        protected override byte[] ComputeUserPasswordHash(byte[] encryptionKey)
        {
            var md5 = MD5.Create();
            md5.AddData(BytePadder.PdfPasswordPaddingBytes);
            md5.AddData(Parameters.IdFirstElement);
            md5.FinalizeHash();
            var hash = md5.Hash;
            var rc4 = new RC4(encryptionKey);
            rc4.TransfromInPlace(hash);
            for (int i = 1; i <= 19; i++)
            {
                var loopRc4 = new RC4(RoundKey(encryptionKey, i));
                loopRc4.TransfromInPlace(hash);
            }

            var ret = new byte[32];
            hash.AsSpan().CopyTo(ret);
            hash.AsSpan().CopyTo(ret.AsSpan(16));
            return ret;
        }

        private byte[] RoundKey(byte[] encryptionKey, int iteration)
        {
            var ret = new byte[encryptionKey.Length];
            for (int i = 0; i < ret.Length; i++)
            {
                ret[i] = (byte) (encryptionKey[i] ^ iteration);
            }

            return ret;
        }

        protected override bool CompareUserHash(in ReadOnlySpan<byte> a, in ReadOnlySpan<byte> b) => 
            base.CompareUserHash(a[..16], b[..16]);
    }
}