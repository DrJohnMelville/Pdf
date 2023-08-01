using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Melville.Pdf.LowLevel.Model.Objects;

namespace Melville.Pdf.Model.OptionalContent;

/// <summary>
/// Controls the display of optional content within a PDF page.
/// </summary>
public interface IOptionalContentState
{
    /// <summary>
    /// Given a content state group, determines if the group should be displayed
    /// </summary>
    /// <param name="dictionary">The content state group dictionary inquired about</param>
    /// <returns>True if the item should be displayed, false otherwise.</returns>
    ValueTask<bool> IsGroupVisibleAsync(PdfValueDictionary? dictionary);

    /// <summary>
    /// A list of optional content group configurations supported by the document.
    /// </summary>
    IReadOnlyList<OptionalContentConfiguration> Configurations { get; }
    
    /// <summary>
    /// Construct a tree of OptionalContentDisplayGroup objects, suitable for display in a user interface.
    /// </summary>
    /// <param name="order">The order array from an optional content group.</param>
    /// <returns></returns>
    ValueTask<IReadOnlyList<IOptionalContentDisplayGroup>> ConstructUiModelAsync(PdfValueArray? order);

    /// <summary>
    /// Indicates when the selected visible content has changed.
    /// </summary>
    event EventHandler<EventArgs>? SelectedContentChanged; 
    /// <summary>
    /// The currently selected visibility configuration.
    /// </summary>
    OptionalContentConfiguration? SelectedConfiguration { get; set; }
    /// <summary>
    /// Make all optional content visible.
    /// </summary>
    /// <returns></returns>
    bool AllVisible();
}