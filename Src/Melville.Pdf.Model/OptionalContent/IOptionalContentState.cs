using System.Collections.Generic;
using System.Threading.Tasks;
using Melville.Pdf.LowLevel.Model.Objects;

namespace Melville.Pdf.Model.OptionalContent;

public interface IOptionalContentState
{
    public ValueTask<bool> IsGroupVisible(PdfDictionary? dictionary);
}