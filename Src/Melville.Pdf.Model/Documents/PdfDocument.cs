using System.Threading.Tasks;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Document;
using Melville.Pdf.LowLevel.Model.Objects;

namespace Melville.Pdf.Model.Documents
{
    public readonly struct PdfDocument
    {
        public PdfLowLevelDocument LowLevelDocument { get; }

        public PdfDocument(PdfLowLevelDocument lowLevelDocument)
        {
            LowLevelDocument = lowLevelDocument;
        }

        private ValueTask<PdfDictionary> CatalogAsync() => 
            LowLevelDocument.TrailerDictionary.GetAsync<PdfDictionary>(KnownNames.Root);

        public async ValueTask<PdfName> VersionAsync() =>
            (await CatalogAsync()).TryGetValue(KnownNames.Version, out var task) &&
                (await task) is PdfName version?
                version: KnownNames.Get($"{LowLevelDocument.MajorVersion}.{LowLevelDocument.MinorVersion}");
    }
}