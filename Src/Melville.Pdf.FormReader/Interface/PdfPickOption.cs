using Melville.INPC;
using Melville.Pdf.LowLevel.Model.Objects;

namespace Melville.Pdf.FormReader.Interface
{
    /// <summary>
    /// Represents a single option in a PDF Pick Control
    /// </summary>
    public partial class PdfPickOption
    {
        /// <summary>
        /// The value to display for this option
        /// </summary>
        [FromConstructor] public string Title { get; }
        /// <summary>
        /// The value written to this control's V field when the option is selected.
        /// </summary>
        [FromConstructor] public PdfDirectObject Value { get; }
    }
}