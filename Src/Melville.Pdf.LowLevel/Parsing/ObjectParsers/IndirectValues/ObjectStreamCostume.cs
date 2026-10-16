using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Objects;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Melville.Pdf.LowLevel.Parsing.ObjectParsers.IndirectValues;

internal readonly struct ObjectStreamCostume(PdfStream inner)
{
    public ValueTask<int> CountAsync() => inner.GetOrDefaultAsync(KnownNames.N, 0);
    public ValueTask<int> FirstValueOffsetAsync() => inner.GetOrDefaultAsync(KnownNames.First, 0);
    public ValueTask<Stream> StreamAsync() => inner.StreamContentAsync();
}
