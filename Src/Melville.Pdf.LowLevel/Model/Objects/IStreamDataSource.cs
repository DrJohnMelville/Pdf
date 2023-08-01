using System.IO;
using System.Threading.Tasks;
using Melville.Pdf.LowLevel.Filters.FilterProcessing;
using Melville.Pdf.LowLevel.Model.Objects2;

namespace Melville.Pdf.LowLevel.Model.Objects;

internal interface IStreamDataSource
{
    ValueTask<Stream> OpenRawStreamAsync(long streamLength);
    Stream WrapStreamWithDecryptor(Stream encryptedStream, PdfDirectValue cryptFilterName);
    Stream WrapStreamWithDecryptor(Stream encryptedStream);
    StreamFormat SourceFormat { get; }
}