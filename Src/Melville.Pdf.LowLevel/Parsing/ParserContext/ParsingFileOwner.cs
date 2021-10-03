using System;
using System.IO;
using System.IO.Pipelines;
using System.Threading.Tasks;
using Melville.Pdf.LowLevel.Encryption;
using Melville.Pdf.LowLevel.Encryption.CryptContexts;
using Melville.Pdf.LowLevel.Encryption.SecurityHandlers;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Parsing.Decryptors;
using Melville.Pdf.LowLevel.Parsing.ObjectParsers;
using EncryptingParsingReader = Melville.Pdf.LowLevel.Encryption.CryptContexts.EncryptingParsingReader;

namespace Melville.Pdf.LowLevel.Parsing.ParserContext
{
    public partial class ParsingFileOwner
    {
        private readonly Stream source;
        private long preHeaderOffset = 0;
        public long StreamLength => source.Length;
        public IPdfObjectParser RootObjectParser { get; }
        public IIndirectObjectResolver IndirectResolver { get; }
        private IDocumentCryptContext documentCryptContext = NullSecurityHandler.Instance;
        private IPasswordSource passwordSource;

        public ParsingFileOwner(
            Stream source,
            IPasswordSource? passwordSource = null,
            IPdfObjectParser? rootObjectParser = null, 
            IIndirectObjectResolver? indirectResolver = null)
        {
            this.source = source;
            this.passwordSource = passwordSource ?? new NullPasswordSource();
            RootObjectParser = rootObjectParser ?? new PdfCompositeObjectParser();
            IndirectResolver = indirectResolver ?? new IndirectObjectResolver();
            if (!source.CanSeek) throw new PdfParseException("PDF Parsing requires a seekable stream");
        }

        private object? currentReader = null;

        public void SetPreheaderOffset(long offset) => preHeaderOffset = offset;
        private void SeekToRentedOrigin(long offset)
        {
            // we may eventually want a multithreaed version of this so we can load multiple pages on different
            // threads
            if (currentReader != null) throw new InvalidOperationException("May only create one reader at a time");
            source.Seek(offset + preHeaderOffset, SeekOrigin.Begin);
        }

        public ValueTask<IParsingReader> RentReader(long offset, int objectNumber=-1, int generation = -1)
        {
            SeekToRentedOrigin(offset);
            var reader = ParsingReaderForStream(source, offset);
            currentReader = reader;
            return new ValueTask<IParsingReader>(TryWrapWithDecryptor(objectNumber, generation, reader));
            
        }

        private IParsingReader TryWrapWithDecryptor(int objectNumber, int generation, IParsingReader reader)
        {
            return objectNumber > 0 && generation >= 0
                ? new EncryptingParsingReader(reader,
                    documentCryptContext.ContextForObject(objectNumber, generation))
                : reader;
        }

        public IParsingReader ParsingReaderForStream(Stream s, long position) =>
            new ParsingReader(this, PipeReader.Create(s, pipeOptions), position);

        public ValueTask<Stream> RentStream(long position, long length)
        {
            SeekToRentedOrigin(position);
            var ret = new RentedStream(source, length, this);
            currentReader = ret;
            return new ValueTask<Stream>(ret);
        }

        private static readonly StreamPipeReaderOptions pipeOptions = new(leaveOpen: true);

        public void ReturnReader(object item)
        {
            if (item == currentReader) currentReader = null;
        }

        public async ValueTask InitializeDecryption(PdfDictionary trailerDictionary)
        {
            if (AlreadyInitializedDecryption()) return;
            documentCryptContext = await 
                TrailerToDocumentCryptContext.CreateDecryptorFactory(trailerDictionary, passwordSource);
        }

        private bool AlreadyInitializedDecryption()
        {
            return documentCryptContext != NullSecurityHandler.Instance;
        }
    }
}