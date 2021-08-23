using System;
using System.Threading.Tasks;
using Melville.Pdf.ReferenceDocumentGenerator.ArgumentParsers;
using Melville.Pdf.ReferenceDocumentGenerator.DocumentTypes.LowLevel;

namespace Melville.Pdf.ReferenceDocumentGenerator
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("Melville.Pdf.ReferenceDocumentGenerator: Generate PDF documents exercising the PDF standard.");
            Console.WriteLine("-H for help");
            var parser = CreateParser();
            foreach (var arg in args)
            {
                await parser.Parse(arg);
            }
        }

        private static IRootParser CreateParser() =>
            new RootArgumentParser(
                new CompositeParser(new IArgumentParser[]{
                    new MinimalPdfParser(),
                    new FiltersGenerator(),
                    new ObjectStreamPage(),
                    new HelpPasrser(),
                    new FileTargetParser(),
                }));
    }
}