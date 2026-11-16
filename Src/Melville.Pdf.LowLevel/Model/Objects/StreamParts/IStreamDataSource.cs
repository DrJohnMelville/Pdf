using System.IO;
using System.Threading.Tasks;
using Melville.Pdf.LowLevel.Filters.FilterProcessing;

namespace Melville.Pdf.LowLevel.Model.Objects.StreamParts;

internal interface IStreamDataSource
{
    IPdfStreamReader OpenRawReader(long streamLength);
    Stream WrapStreamWithDecryptor(Stream encryptedStream, PdfDirectObject cryptFilterName);
    Stream WrapStreamWithDecryptor(Stream encryptedStream);
    StreamFormat SourceFormat { get; }
}