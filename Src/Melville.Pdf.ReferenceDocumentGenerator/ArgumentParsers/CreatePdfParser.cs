using System;
using System.IO;
using System.Threading.Tasks;

namespace Melville.Pdf.ReferenceDocumentGenerator.ArgumentParsers
{
    public abstract class CreatePdfParser: IArgumentParser
    {
        public string Prefix { get; }
        public string HelpText { get; }

        protected CreatePdfParser(string prefix, string helpText)
        {
            Prefix = prefix;
            HelpText = helpText;
        }

        public async ValueTask<IArgumentParser?> ParseArgumentAsync(string argument, IRootParser root)
        {
            Console.WriteLine("Generating: " + Prefix);
            await using (var targetStream = root.Target.CreateTargetStream())
            {
                await WritePdfAsync(targetStream);
            }
            root.Target.View();
            return null;
        }

        public abstract ValueTask WritePdfAsync(Stream target);
    }
}