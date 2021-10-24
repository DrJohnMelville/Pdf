using System;
using System.Threading.Tasks;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Document;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Model.Objects.StringEncodings;

namespace Melville.Pdf.Model.Documents
{
    public readonly struct PdfDocument
    {
        public PdfLowLevelDocument LowLevel { get; }

        public PdfDocument(PdfLowLevelDocument lowLevel)
        {
            LowLevel = lowLevel;
        }

        private ValueTask<PdfDictionary> CatalogAsync() => 
            LowLevel.TrailerDictionary.GetAsync<PdfDictionary>(KnownNames.Root);

        public async ValueTask<PdfName> VersionAsync() =>
            (await CatalogAsync()).TryGetValue(KnownNames.Version, out var task) &&
                (await task) is PdfName version?
                version: NameDirectory.Get($"{LowLevel.MajorVersion}.{LowLevel.MinorVersion}");

        public async ValueTask<PageTree> PagesAsync() =>
            new(await (await CatalogAsync()).GetAsync<PdfDictionary>(KnownNames.Pages));
    }
}