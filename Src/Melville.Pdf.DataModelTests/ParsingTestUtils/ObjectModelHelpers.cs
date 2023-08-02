using System;
using Melville.Pdf.LowLevel.Model.Objects;

namespace Melville.Pdf.DataModelTests.ParsingTestUtils;

public static class ObjectModelHelpers
{
    public static T ForceTo<T>(this PdfIndirectValue obj)
    {
        if (!obj.TryGetEmbeddedDirectValue(out T ret))
            throw new InvalidOperationException("Value was not expected type");
        return ret;
    }
}