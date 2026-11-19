using System.IO;
using System.Threading.Tasks;
using Melville.Pdf.LowLevel.Filters.FilterProcessing;

namespace Melville.Pdf.LowLevel.Model.Objects.StreamParts;

internal interface IStreamDataSource
{
    IPdfStreamReader OpenRawReader(long streamLength);
    IPdfStreamReader WrapStreamWithDecryptor(IPdfStreamReader encryptedStream, PdfDirectObject cryptFilterName);
    IPdfStreamReader WrapStreamWithDecryptor(IPdfStreamReader encryptedStream);
    StreamFormat SourceFormat { get; }
}