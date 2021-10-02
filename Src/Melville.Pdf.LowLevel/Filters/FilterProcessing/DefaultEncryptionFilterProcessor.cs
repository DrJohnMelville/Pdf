using System.IO;
using System.Threading.Tasks;
using Melville.Pdf.LowLevel.Model.Objects;

namespace Melville.Pdf.LowLevel.Filters.FilterProcessing
{
    public class DefaultEncryptionFilterProcessor : FilterProcessorBase
    {
        private readonly IFilterProcessor innerProcessor;
        private readonly IStreamDataSource streamSource;
        private readonly IObjectCryptContext encryptor;

        public DefaultEncryptionFilterProcessor(IFilterProcessor innerProcessor, IStreamDataSource streamSource,
            IObjectCryptContext encryptor)
        {
            this.innerProcessor = innerProcessor;
            this.streamSource = streamSource;
            this.encryptor = encryptor;
        }

        protected override async ValueTask<Stream> Encode(
            Stream source, StreamFormat sourceFormat, StreamFormat targetFormat) =>
            TryEncrypt(await innerProcessor.StreamInDesiredEncoding(source, sourceFormat, targetFormat),
                sourceFormat, targetFormat);

        private Stream TryEncrypt(
            Stream source, StreamFormat sourceFormat, StreamFormat targetFormat) =>
            ShouldEncrypt(sourceFormat, targetFormat) ? 
                encryptor.StreamCipher().Encrypt().CryptStream(source)
                : source;

        private static bool ShouldEncrypt(
            StreamFormat sourceFormat, StreamFormat targetFormat) =>
            sourceFormat >= StreamFormat.ImplicitEncryption &&
            targetFormat < StreamFormat.ImplicitEncryption;

        protected override ValueTask<Stream> Decode(
            Stream source, StreamFormat sourceFormat, StreamFormat targetFormat) =>
            innerProcessor.StreamInDesiredEncoding(
                TryDecrypt(source, sourceFormat, targetFormat), sourceFormat, targetFormat);

        private Stream TryDecrypt(Stream source, StreamFormat sourceFormat, StreamFormat targetFormat) =>
            ShouldDecrypt(sourceFormat, targetFormat)
                ? streamSource.WrapStreamWithDecryptor(source)
                : source;

        private static bool ShouldDecrypt(StreamFormat sourceFormat, StreamFormat targetFormat) =>
            sourceFormat < StreamFormat.ImplicitEncryption &&
            targetFormat >= StreamFormat.ImplicitEncryption;
    }
}