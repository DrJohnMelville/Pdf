using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Melville.FileSystem;
using Melville.Pdf.LowLevel.Model.Document;
using Melville.Pdf.LowLevel.Parsing.FileParsers;

namespace Melville.Pdf.LowLevelReader.DocumentParts
{
    public interface IPartParser
    {
        Task<DocumentPart[]> ParseAsync(IFile source);
    }
    public class PartParser: IPartParser
    {
        private ViewModelVisitor generator = new();
        private List<DocumentPart> items = new();
        public async Task<DocumentPart[]> ParseAsync(IFile source)
        {
            PdfLowLevelDocument lowlevel = await RandomAccessFileParser.Parse(await source.OpenRead());
            GenerateHeaderElement(lowlevel);
            foreach (var item in lowlevel.Objects.Values
                .OrderBy(i=>i.Target.ObjectNumber)
                .ToList())
            {
                items.Add(await item.Target.Visit(generator));
            }
            items.Add(await generator.GeneratePart("Trailer: ", lowlevel.TrailerDictionary));
            return items.ToArray();
        }

        private void GenerateHeaderElement(PdfLowLevelDocument lowlevel) =>
            items.Add(new DocumentPart($"PDF-{lowlevel.MajorVersion}.{lowlevel.MinorVersion}"));
    }
}