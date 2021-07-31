using System.Threading.Tasks;

namespace Melville.Pdf.ReferenceDocumentGenerator.ArgumentParsers
{
    public interface IArgumentParser
    {
        public string Prefix { get; }
        ValueTask<IArgumentParser?> ParseArgumentAsync(string argument, IRootParser root);
    }
}