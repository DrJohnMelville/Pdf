using System.Collections.Generic;
using System.Data.Common;
using System.Globalization;
using System.Threading.Tasks;
using Melville.FileSystem;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Visitors;

namespace Melville.Pdf.LowLevelReader.DocumentParts
{
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
            //Notice there is an ordering dependency in these two declarationsn.  The second
            //line changes prefix;
            var dictionaryTitle = prefix + "Dictionary";
            var children = await ParseDictionaryChildren(item);
            return new DocumentPart(dictionaryTitle, children);
        }

        private async Task<DocumentPart[]> ParseDictionaryChildren(PdfDictionary item)
        {
            var children = new DocumentPart[item.Count];
            var next = 0;
            foreach (var child in item.RawItems)
            {
                children[next++] = await GenerateDictionaryItem(child);
            }

            return children;
        }

        private ValueTask<DocumentPart> GenerateDictionaryItem(KeyValuePair<PdfName, PdfObject> item) => 
            GeneratePart($"{item.Key}: ", item.Value);

        public ValueTask<DocumentPart> Visit(PdfTokenValues item) => Terminal(item.ToString());

        public async ValueTask<DocumentPart> Visit(PdfIndirectObject item)
        {
            var title = $"{prefix}{item.ObjectNumber} {item.GenerationNumber} obj";
            prefix = "";
            return new DocumentPart(title, new[] {await (await item.DirectValue()).Visit(this)});
        }

        public ValueTask<DocumentPart> Visit(PdfIndirectReference item) => 
            Terminal ($"{item.Target.ObjectNumber} {item.Target.GenerationNumber} R");

        public ValueTask<DocumentPart> Visit(PdfName item) => Terminal(item.ToString());

        public ValueTask<DocumentPart> Visit(PdfInteger item) => Terminal(item.IntValue.ToString());

        public ValueTask<DocumentPart> Visit(PdfDouble item) => 
            Terminal(item.DoubleValue.ToString(CultureInfo.CurrentUICulture));

        public ValueTask<DocumentPart> Visit(PdfString item) => Terminal($"({item})");

        public async ValueTask<DocumentPart> Visit(PdfStream item)
        {
            var title = prefix + "Stream";
            var children = await ParseDictionaryChildren(item);
            return new StreamDocumentPart(title, children, item);
        }

        public ValueTask<DocumentPart> Visit(PdfFreeListObject item) =>
            Terminal($"Deleted Slot. Next: " + item.NextItem);
    }
}