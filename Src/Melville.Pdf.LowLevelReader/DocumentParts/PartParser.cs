using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using Melville.FileSystem;
using Melville.Pdf.LowLevel.Model.Document;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Parsing.FileParsers;
using Melville.Pdf.LowLevel.Visitors;

namespace Melville.Pdf.LowLevelReader.DocumentParts
{
    public interface IPartParser
    {
        Task<DocumentPart[]> Parse(IFile source);
    }
    public class PartParser: IPartParser
    {
        private ViewModelVisitor generator = new();
        private List<DocumentPart> items = new();
        public async Task<DocumentPart[]> Parse(IFile source)
        {
            PdfLowLevelDocument lowlevel = await RandomAccessFileParser.Parse(await source.OpenRead());
            GenerateHeaderElement(lowlevel);
            foreach (var item in lowlevel.Objects.Values)
            {
                items.Add(await item.Target.Visit(generator));
            }
            items.Add(await generator.GeneratePart("Trailer: ", lowlevel.TrailerDictionary));
            return items.ToArray();
        }

        private void GenerateHeaderElement(PdfLowLevelDocument lowlevel) =>
            items.Add(new DocumentPart($"PDF-{lowlevel.MajorVersion}.{lowlevel.MinorVersion}"));
    }

    public class ViewModelVisitor : ILowLevelVisitor<ValueTask<DocumentPart>>
    {
        private string prefix = "";

        public ValueTask<DocumentPart> GeneratePart(string newPrefix, PdfObject item)
        {
            prefix = newPrefix;
            return item.Visit(this);
        }

        private ValueTask<DocumentPart> Terminal(string text)
        {
            var ret = new ValueTask<DocumentPart>(new DocumentPart(prefix + text));
            prefix = "";
            return ret;
        }

        public async ValueTask<DocumentPart> Visit(PdfArray item)
        {
            var title = prefix + "Array";
            var children = new DocumentPart[item.Count];
            for (int i = 0; i < children.Length; i++)
            {
                children[i] = await GeneratePart($"[{i}]: ", item.RawItems[i]);
            }

            return new DocumentPart(title, children);
        }

        public ValueTask<DocumentPart> Visit(PdfBoolean item) => Terminal(item.ToString());

        public async ValueTask<DocumentPart> Visit(PdfDictionary item)
        {
            var capturePrefix = prefix;
            var children = new DocumentPart[item.Count];
            var next = 0;
            foreach (var child in item.RawItems)
            {
                children[next++] = await GenerateDictionaryItem(child);
            }
            return new DocumentPart(capturePrefix + "Dictionary", children);
        }

        private ValueTask<DocumentPart> GenerateDictionaryItem(KeyValuePair<PdfName, PdfObject> item) => 
            GeneratePart($"/{item.Key}: ", item.Value);

        public ValueTask<DocumentPart> Visit(PdfTokenValues item) => Terminal(item.ToString());

        public async ValueTask<DocumentPart> Visit(PdfIndirectObject item)
        {
            var title = $"{prefix}{item.ObjectNumber} {item.GenerationNumber} obj";
            prefix = "";
            return new DocumentPart(title, new[] {await (await item.DirectValue()).Visit(this)});
        }

        public ValueTask<DocumentPart> Visit(PdfIndirectReference item) => 
            Terminal ($"{item.Target.ObjectNumber} {item.Target.GenerationNumber} R");

        public ValueTask<DocumentPart> Visit(PdfName item) => Terminal("/" + item);

        public ValueTask<DocumentPart> Visit(PdfInteger item) => Terminal(item.IntValue.ToString());

        public ValueTask<DocumentPart> Visit(PdfDouble item) => 
            Terminal(item.DoubleValue.ToString(CultureInfo.CurrentUICulture));

        public ValueTask<DocumentPart> Visit(PdfString item)
        {
            throw new System.NotImplementedException();
        }

        public ValueTask<DocumentPart> Visit(PdfStream item)
        {
            return Visit((PdfDictionary) item);
        }
    }
}