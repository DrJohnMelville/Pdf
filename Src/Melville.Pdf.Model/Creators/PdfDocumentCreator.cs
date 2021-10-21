using System.Collections.Generic;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Document;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Writers.Builder;

namespace Melville.Pdf.Model.Creators
{
    public class PdfDocumentCreator
    {
        public ILowLevelDocumentCreator LowLevelCreator { get; } = new LowLevelDocumentCreator();
        private readonly Dictionary<PdfName, PdfObject> catalogItems = new();

        public PdfLowLevelDocument CreateDocument()
        {
            LowLevelCreator.AddToTrailerDictionary(KnownNames.Root, LowLevelCreator.Add(
                new PdfDictionary(catalogItems)));
            return LowLevelCreator.CreateDocument();
        }

        public void SetVersionInCatalog(byte major, byte minor) =>
            SetVersionInCatalog(KnownNames.Get($"{major}.{minor}"));
        public void SetVersionInCatalog(PdfName version)
        {
            catalogItems[KnownNames.Version] = version;
        }
    }
}