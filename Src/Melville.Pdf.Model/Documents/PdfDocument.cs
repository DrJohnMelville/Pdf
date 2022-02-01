using System;
using System.IO;
using System.Threading.Tasks;
using Melville.Parsing.AwaitConfiguration;
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
            await RandomAccessFileParser.Parse(new ParsingFileOwner(source, passwords)).CA());

    public PdfDocument(PdfLowLevelDocument lowLevel)
    {
        LowLevel = lowLevel;
    }

    private ValueTask<PdfDictionary> CatalogAsync() => 
        LowLevel.TrailerDictionary.GetAsync<PdfDictionary>(KnownNames.Root);

    public async ValueTask<PdfName> VersionAsync() =>
        (await CatalogAsync().CA()).TryGetValue(KnownNames.Version, out var task) &&
        (await task.CA()) is PdfName version?
            version: NameDirectory.Get($"{LowLevel.MajorVersion}.{LowLevel.MinorVersion}");

    public async ValueTask<PageTree> PagesAsync() =>
        new(await (await CatalogAsync().CA()).GetAsync<PdfDictionary>(KnownNames.Pages).CA());
}