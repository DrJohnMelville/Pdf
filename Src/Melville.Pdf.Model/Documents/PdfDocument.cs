using System;
using System.IO;
using System.Threading.Tasks;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Document;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Model.Objects.StringEncodings;
using Melville.Pdf.LowLevel.Parsing.FileParsers;
using Melville.Pdf.LowLevel.Parsing.ParserContext;

namespace Melville.Pdf.Model.Documents;

public readonly struct PdfDocument
{
    public PdfLowLevelDocument LowLevel { get; }

    public static async ValueTask<PdfDocument> ReadAsync(Stream source, IPasswordSource? passwords = null) => 
        new(
            await RandomAccessFileParser.Parse(new ParsingFileOwner(source, passwords)).ConfigureAwait(false));

    public PdfDocument(PdfLowLevelDocument lowLevel)
    {
        LowLevel = lowLevel;
    }

    private ValueTask<PdfDictionary> CatalogAsync() => 
        LowLevel.TrailerDictionary.GetAsync<PdfDictionary>(KnownNames.Root);

    public async ValueTask<PdfName> VersionAsync() =>
        (await CatalogAsync().ConfigureAwait(false)).TryGetValue(KnownNames.Version, out var task) &&
        (await task.ConfigureAwait(false)) is PdfName version?
            version: NameDirectory.Get($"{LowLevel.MajorVersion}.{LowLevel.MinorVersion}");

    public async ValueTask<PageTree> PagesAsync() =>
        new(await (await CatalogAsync().ConfigureAwait(false)).GetAsync<PdfDictionary>(KnownNames.Pages).ConfigureAwait(false));
}