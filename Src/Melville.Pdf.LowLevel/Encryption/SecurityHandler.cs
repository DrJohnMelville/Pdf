using System;

namespace Melville.Pdf.LowLevel.Encryption
{
    public interface ISecurityHandler
    {
        byte[] UserPasswordHash(in Span<byte> plaintextPassword);
    }
    public class SecurityHandlerV2: ISecurityHandler
    {
        public byte[] UserPasswordHash(in Span<byte> plaintextPassword)
        {
            throw new NotImplementedException();
        }
    }
    
    public class SecurityHandlerV3: ISecurityHandler
    {
        public byte[] UserPasswordHash(in Span<byte> plaintextPassword)
        {
            throw new NotImplementedException();
        }
    }
}