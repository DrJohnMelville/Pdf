using System.IO;
using System.Threading.Tasks;
using Melville.Pdf.LowLevel.Model.Objects2;

namespace Melville.Pdf.LowLevel.Filters.FilterProcessing;

internal interface IApplySingleFilter
{
    ValueTask<Stream> EncodeAsync(Stream source, PdfDirectValue filter, PdfDirectValue parameter);
    ValueTask<Stream> DecodeAsync(Stream source, PdfDirectValue filter, PdfDirectValue parameter);
}