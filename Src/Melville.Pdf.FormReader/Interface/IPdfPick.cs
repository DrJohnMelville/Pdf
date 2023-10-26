namespace Melville.Pdf.FormReader.Interface;

/// <summary>
/// Represent PDF Pick form controls.
/// </summary>
public interface IPdfPick : IPdfFormField
{
    /// <summary>
    /// List of options to pick among.
    /// </summary>
    IReadOnlyList<PdfPickOption> Options { get; }
}