﻿using System.Threading.Tasks;
using Melville.Pdf.LowLevel.Model.Objects;

namespace Melville.Pdf.LowLevel.Model.ContentStreams;

/// <summary>
/// This is a composite interface of all the operations that can be done from a
/// content stream.  It inherits from a number of other interfaces to facilitate
/// forwarding in the content stream parser.
/// </summary>
public interface IContentStreamOperations: 
    IStateChangingOperations, IDrawingOperations, IColorOperations, 
    ITextObjectOperations, ITextBlockOperations, IMarkedContentCSOperations, ICompatibilityOperations,
    IFontMetricsOperations
{
    /// <summary>
    /// Content stream operator gs
    /// </summary>
    ValueTask LoadGraphicStateDictionaryAsync(PdfDirectObject dictionaryName);
}