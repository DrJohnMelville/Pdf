using System.Threading.Tasks;
using Melville.Pdf.ReferenceDocumentGenerator.Targets;

namespace Melville.Pdf.ReferenceDocumentGenerator.ArgumentParsers
{
    public class ViewTargetParser: IArgumentParser
    {
        public string Prefix => "-v";

        public ValueTask<IArgumentParser?> ParseArgumentAsync(string argument, IRootParser root)
        {
            root.Target = new FileTarget(argument);
            return new ValueTask<IArgumentParser?>();
        }
    }
}