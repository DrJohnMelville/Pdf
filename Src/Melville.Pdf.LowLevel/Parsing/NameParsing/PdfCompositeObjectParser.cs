using System.Buffers;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO.Pipelines;
using System.Linq;
using System.Threading.Tasks;
using Melville.Pdf.LowLevel.Model;
using Melville.Pdf.LowLevel.Parsing.StringParsing;

namespace Melville.Pdf.LowLevel.Parsing.NameParsing
{
    public abstract class IPdfObjectParser
    {
//        bool TryParse(ref SequenceReader<byte> reader, [NotNullWhen(true)] out PdfObject? obj);
        public abstract Task<PdfObject> ParseAsync(PipeReader pr);

    }

    public interface IWantRoot
    {
        void SetRoot(IPdfObjectParser rootParser);
    }

    public class PdfCompositeObjectParser: PdfAtomParser
    {
        private readonly IDictionary<byte, IPdfObjectParser> catalog;

        public PdfCompositeObjectParser(IDictionary<byte, IPdfObjectParser>? catalog = null)
        {
            this.catalog = catalog ?? DefaultObjectParserCatalog.instance;
            InformChildrenOfRootParser();
        }

        private void InformChildrenOfRootParser()
        {
            foreach (var subParser in catalog.Values)
            {
                (subParser as IWantRoot)?.SetRoot(this);
            }
        }

        public override bool TryParse(ref SequenceReader<byte> reader, out PdfObject? obj)
        {
            #warning this is temporary and broken to assume PdfAtomParser
            if (reader.TryPeek(out byte first))
                return ((PdfAtomParser)catalog[first]).TryParse(ref reader, out obj);
                    
            obj = null;
            return false;
        }
    }

    public class DefaultObjectParserCatalog : Dictionary<byte, IPdfObjectParser>
    {
        public static readonly DefaultObjectParserCatalog instance = new();
        private DefaultObjectParserCatalog()
        {
            Add('<', new HexStringParser());
            Add('(', new SyntaxStringParser());
            Add('[', new PdfArrayParser());
            RegisterNumParser();
            DefineNames();
            DefineLiterals();
        }

        private void RegisterNumParser()
        {
            var numParser = new NumberParser();
            for (char i = '0'; i <= '9'; i++)
            {
                Add(i, numParser);
            }
            Add('-', numParser);
            Add('+', numParser);
        }

        private void DefineNames()
        {
            Add('/', new NameParser());
        }

        private void DefineLiterals()
        {
            Add('t', new LiteralTokenParser(4, PdfBoolean.True));
            Add('f', new LiteralTokenParser(5, PdfBoolean.False));
            Add('n', new LiteralTokenParser(4, PdfNull.Instance));
        }

        private void Add(char c, IPdfObjectParser parser) => Add((byte) c, parser);
    }
}