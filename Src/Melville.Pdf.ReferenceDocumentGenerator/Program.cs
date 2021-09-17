using System;
using System.Collections.Generic;
using System.Linq;
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
                new CompositeParser(FindParsers()));

        private static IReadOnlyList<IArgumentParser> FindParsers()
        {
            var ret = new List<IArgumentParser>()
            {
                new FileTargetParser(),
                new ViewTargetParser(),
            };
            ret.Add(new HelpPasrser(ret));
            ret.Add(new MinimalPdfParser());
            ret.Add(new FiltersGenerator());
            ret.Add(new ObjectStreamPage());
            return ret;
        }
    }
}