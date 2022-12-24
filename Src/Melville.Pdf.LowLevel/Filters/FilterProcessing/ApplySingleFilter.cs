using System.IO;
using System.Threading.Tasks;
using Melville.Pdf.LowLevel.Model.Objects;

namespace Melville.Pdf.LowLevel.Filters.FilterProcessing;

internal interface IApplySingleFilter
{
    ValueTask<Stream> Encode(Stream source, PdfObject filter, PdfObject parameter);
    ValueTask<Stream> Decode(Stream source, PdfObject filter, PdfObject parameter);
}