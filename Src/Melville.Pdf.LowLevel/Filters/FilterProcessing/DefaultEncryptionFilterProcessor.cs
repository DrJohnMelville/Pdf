using System;
using System.IO;
using System.Threading.Tasks;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Objects;

namespace Melville.Pdf.LowLevel.Filters.FilterProcessing
{
    public class DefaultEncryptionFilterProcessor : FilterProcessorBase
    {
        private readonly IFilterProcessor innerProcessor;
        private readonly IStreamDataSource streamSource;

        public DefaultEncryptionFilterProcessor(IFilterProcessor innerProcessor, IStreamDataSource streamSource)
        {
            this.innerProcessor = innerProcessor;
            this.streamSource = streamSource;
        }

        protected override ValueTask<Stream> Encode(
            Stream source, StreamFormat sourceFormat, StreamFormat targetFormat) =>
            innerProcessor.StreamInDesiredEncoding(TryEncrypt(source, sourceFormat, targetFormat),
                sourceFormat, targetFormat);

        private static Stream TryEncrypt(
            Stream source, StreamFormat sourceFormat, StreamFormat targetFormat) =>
            ShouldEncrypt(sourceFormat, targetFormat) ?
                //innerProcessor.StreamInDesiredEncoding(streamSource.Encrypt(source, ))
                throw new NotImplementedException("Need to handle stream encryption")
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
                ? streamSource.WrapStreamWithDecryptor(source, KnownNames.StmF)
                : source;

        private static bool ShouldDecrypt(StreamFormat sourceFormat, StreamFormat targetFormat) =>
            sourceFormat < StreamFormat.ImplicitEncryption &&
            targetFormat >= StreamFormat.ImplicitEncryption;
    }
}