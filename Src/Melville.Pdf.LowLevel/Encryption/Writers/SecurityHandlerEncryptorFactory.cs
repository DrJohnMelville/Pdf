﻿using System;
using System.Threading.Tasks;
using Melville.Pdf.LowLevel.Encryption.Readers;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Parsing.ParserContext;
using Melville.Pdf.LowLevel.Writers.DocumentWriters;

namespace Melville.Pdf.LowLevel.Encryption.Writers
{
    public static class SecurityHandlerEncryptorFactory
    {
        public static async ValueTask<IDocumentEncryptor> CreateDocumentEncryptor(
            PdfDictionary trailer, string? userPassword)
        {
            if (await trailer.GetOrNullAsync(KnownNames.Encrypt) is not PdfDictionary dict)
                return NullDocumentEncryptor.Instance;
            var securityHandler = await SecurityHandlerFactory.CreateSecurityHandler(trailer, dict);
            if (!securityHandler.TrySinglePassword((userPassword, PasswordType.User)))
                throw new ArgumentException("Incorrect user key for encryption");
            return new SecurityHandlerDocumentEncryptor(securityHandler);
        }
    }
    

    public class SecurityHandlerDocumentEncryptor : IDocumentEncryptor
    {
        private ISecurityHandler handler;

        public SecurityHandlerDocumentEncryptor(ISecurityHandler handler)
        {
            this.handler = handler;
        }

        public IObjectEncryptor CreateEncryptor(PdfIndirectObject parentObject, PdfObject target)
        {
            return handler.EncryptorForObject(parentObject, target);
        }
    }
    
    
}