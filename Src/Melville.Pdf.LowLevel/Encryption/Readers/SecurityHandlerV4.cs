using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Parsing.Decryptors;
using Melville.Pdf.LowLevel.Parsing.ParserContext;
using Melville.Pdf.LowLevel.Writers.DocumentWriters;

namespace Melville.Pdf.LowLevel.Encryption.Readers
{
    public class SecurityHandlerV4 : ISecurityHandler
    {
        private readonly Dictionary<PdfName, ISecurityHandler> handlers;
        private readonly ISecurityHandler defaultStringHandler;
        private readonly ISecurityHandler defaultStreamHandler;

        public SecurityHandlerV4(Dictionary<PdfName, ISecurityHandler> handlers,
            PdfName defStringHandlerName, PdfName defStreamHandlerName)
        {
            this.handlers = handlers;
            defaultStringHandler = handlers[defStringHandlerName];
            defaultStreamHandler = handlers[defStreamHandlerName];
        }

        public IDecryptor DecryptorForObject(int objectNumber, int generationNumber, PdfObject target)
        {
            return PickHandler(target).DecryptorForObject(objectNumber, generationNumber, target);
        }

        private ISecurityHandler PickHandler(PdfObject pdfObject) => pdfObject switch
        {
           PdfString => defaultStringHandler,
           PdfStream stream => HandlerForStream(stream),
           _=> throw new ArgumentException("Only strings and streams can be decrypted.")
        };

        private ISecurityHandler HandlerForStream(PdfStream stream)
        {
            #warning need to tesr a stream with a crypt filter.
            var namedFilter = CryptFilterName(stream).GetAwaiter().GetResult();
            return (namedFilter is not null && handlers.TryGetValue(namedFilter, out var handler))
                ? handler
                : defaultStreamHandler;
        }

        private async ValueTask<PdfName?> CryptFilterName(PdfStream stream)
        {
            var filters = (await stream.GetOrNullAsync(KnownNames.Filter)).AsList();
            var filterParams = (await stream.GetOrNullAsync(KnownNames.DecodeParms)).AsList();
            for (int i = 0; i < filters.Count; i++)
            {
                if ((await filters[i].DirectValue()) == KnownNames.Crypt && i < filterParams.Count)
                {
                    var paramDict = (PdfDictionary) (await filterParams[i].DirectValue());
                    return await paramDict.GetOrNullAsync(KnownNames.Name) as PdfName;
                }
            }

            return null;
        }

        public bool TrySinglePassword((string?, PasswordType) password) => 
            handlers.Values.All(i => i.TrySinglePassword(password));

        public IObjectEncryptor EncryptorForObject(PdfIndirectObject parent, PdfObject target) => 
            PickHandler(target).EncryptorForObject(parent, target);
    }

    public class NullSecurityHandler: ISecurityHandler
    {
        public IDecryptor DecryptorForObject(int objectNumber, int generationNumber, PdfObject target) =>
            NullDecryptor.Instance;

        public bool TrySinglePassword((string?, PasswordType) password) => true;
        public IObjectEncryptor EncryptorForObject(PdfIndirectObject parent, PdfObject target) =>
            NullObjectEncryptor.Instance;
    }
}