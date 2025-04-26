using System.IO;
using System.Threading.Tasks;
using Melville.Pdf.LowLevel.Model.Objects;

namespace Melville.Pdf.LowLevel.Filters.FilterProcessing;

internal interface IApplySingleFilter
{
    ValueTask<Stream> EncodeAsync(Stream source, PdfDirectObject filter, PdfDirectObject parameter, object? context);
    ValueTask<Stream> DecodeAsync(Stream source, PdfDirectObject filter, PdfDirectObject parameter, object? context);
}