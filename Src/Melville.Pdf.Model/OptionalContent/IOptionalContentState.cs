using System.Collections.Generic;
using Melville.Pdf.LowLevel.Model.Objects;

namespace Melville.Pdf.Model.OptionalContent;

public interface IOptionalContentState
{
    public bool IsGroupVisible(PdfDictionary dictionary);
}