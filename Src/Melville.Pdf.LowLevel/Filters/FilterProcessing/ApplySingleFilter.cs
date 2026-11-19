using System;
using System.IO;
using System.Threading.Tasks;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.StreamInterfaces;

namespace Melville.Pdf.LowLevel.Filters.FilterProcessing;

internal interface IApplySingleFilter
{
    ValueTask<IPdfStreamReader> EncodeAsync(
        IPdfStreamReader source, PdfDirectObject filter, PdfDirectObject parameter, object? context);

    ValueTask<IPdfStreamReader> DecodeAsync(
        IPdfStreamReader source, PdfDirectObject filter, PdfDirectObject parameter, object? context);
}