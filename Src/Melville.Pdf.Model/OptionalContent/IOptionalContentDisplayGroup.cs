using System.Collections.Generic;

namespace Melville.Pdf.Model.OptionalContent;

/// <summary>
/// This interface abstracts nodes in the optional content display tree in the UP.
/// </summary>
public interface IOptionalContentDisplayGroup
{
    /// <summary>
    /// Name of the optional content to be controlled.
    /// </summary>
    string Name { get; }
    /// <summary>
    /// Records if this content is currently visible.
    /// </summary>
    bool Visible { get; set; }
    /// <summary>
    /// Records if a checkbox should be shown in the UI to change the visibility
    /// of this content.
    /// </summary>
    bool ShowCheck { get; }
    /// <summary>
    /// A list of children of this content.
    /// </summary>
    IReadOnlyList<IOptionalContentDisplayGroup> Children { get; }
}