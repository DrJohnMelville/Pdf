using System.Collections.Generic;
using Melville.INPC;

namespace Melville.Pdf.Model.Renderers.FontRenderings.CMaps;

internal readonly partial struct CMapFactory
{
    [FromConstructor] private readonly IList<ByteRange> data;
}