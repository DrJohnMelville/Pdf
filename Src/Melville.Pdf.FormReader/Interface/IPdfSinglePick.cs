namespace Melville.Pdf.FormReader.Interface;

/// <summary>
/// Represents PDF Pick controls where at most one option can be selected.
/// </summary>
public interface IPdfSinglePick: IPdfPick
{
    /// <summary>
    /// The selected option -- or null if no option is selected.
    /// </summary>
    public PdfPickOption? Selected { get; set; }
}