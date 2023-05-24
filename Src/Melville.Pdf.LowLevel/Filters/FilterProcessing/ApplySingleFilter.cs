using System.IO;
using System.Threading.Tasks;
using Melville.Pdf.LowLevel.Model.Objects;

namespace Melville.Pdf.LowLevel.Filters.FilterProcessing;

internal interface IApplySingleFilter
{
    ValueTask<Stream> EncodeAsync(Stream source, PdfObject filter, PdfObject parameter);
    ValueTask<Stream> DecodeAsync(Stream source, PdfObject filter, PdfObject parameter);
}