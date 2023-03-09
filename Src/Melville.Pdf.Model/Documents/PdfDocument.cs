using System;
using System.IO;
using System.Threading.Tasks;
using Melville.Parsing.AwaitConfiguration;
using Melville.Pdf.LowLevel;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Document;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Parsing.FileParsers;
using Melville.Pdf.LowLevel.Parsing.ParserContext;

namespace Melville.Pdf.Model.Documents;

/// <summary>
/// This is a costume class that represents a PDF document.
/// </summary>
public readonly struct PdfDocument: IDisposable
{
    /// <summary>
    /// The Low Level representation of this document
    /// </summary>
    public PdfLowLevelDocument LowLevel { get; }

    /// <summary>
    /// Convenience method to read a PdfDocument from a stream.
    /// </summary>
    /// <param name="source">A readable and seekable stream containing a PDF document</param>
    /// <param name="passwords">A password source to allow the low level parser to query the user for a password.</param>
    /// <returns></returns>
    public static async ValueTask<PdfDocument> ReadAsync(Stream source, IPasswordSource? passwords = null) =>
        new(await new PdfLowLevelReader(passwords).ReadFromAsync(source).CA());

    /// <summary>
    /// Create a PdfDocument from a PdfLowLevelDocument
    /// </summary>
    /// <param name="lowLevel"></param>
    public PdfDocument(PdfLowLevelDocument lowLevel)
    {
        LowLevel = lowLevel;
    }

    private ValueTask<PdfDictionary> CatalogAsync() => 
        LowLevel.TrailerDictionary.GetAsync<PdfDictionary>(KnownNames.Root);

    /// <summary>
    /// Gets the effective PDF version for this document, prefering the document caalog if it differs from the
    /// file header.
    /// </summary>
    /// <returns>A PDF name representing the header.</returns>
    public async ValueTask<PdfName> VersionAsync() =>
        (await CatalogAsync().CA()).TryGetValue(KnownNames.Version, out var task) &&
        (await task.CA()) is PdfName version?
            version: NameDirectory.Get($"{LowLevel.MajorVersion}.{LowLevel.MinorVersion}");

    /// <summary>
    /// Gets the PageTree representing the pages in the document
    /// </summary>
    public async ValueTask<PageTree> PagesAsync() =>
        new(await (await CatalogAsync().CA()).GetAsync<PdfDictionary>(KnownNames.Pages).CA());

    /// <summary>
    /// Optional content declaration for the document
    /// </summary>
    public async ValueTask<PdfDictionary?> OptionalContentProperties() =>
        await (await CatalogAsync().CA()).GetOrNullAsync<PdfDictionary>(KnownNames.OCProperties).CA();

    /// <inheritdoc />
    public void Dispose()
    {
        (LowLevel as IDisposable)?.Dispose();
    }
}