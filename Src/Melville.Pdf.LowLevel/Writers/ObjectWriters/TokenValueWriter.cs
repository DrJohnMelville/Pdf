using System.IO.Pipelines;
using System.Threading.Tasks;
using Melville.Pdf.LowLevel.Model.Objects;

namespace Melville.Pdf.LowLevel.Writers.ObjectWriters;

internal static class TokenValueWriter
{
    public static ValueTask<FlushResult> WriteAsync(PipeWriter target, PdfTokenValues item)
    {
        target.WriteBytes(item.TokenValue);
        return target.FlushAsync();
    }
}