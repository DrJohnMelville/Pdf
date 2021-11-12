using System.Collections.Generic;
using System.Linq;
using Melville.Pdf.ReferenceDocumentGenerator.ArgumentParsers;

namespace Melville.Pdf.ReferenceDocumentGenerator;

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
            new CompositeParser(AddGeneratorsToList()));

    private static IReadOnlyList<IArgumentParser> AddGeneratorsToList()
    {
        var ret = ListWithCommands();
        AddGeneratorsToList(ret);
        return ret;
    }

    private static List<IArgumentParser> ListWithCommands()
    {
        var ret = new List<IArgumentParser>()
        {
            new FileTargetParser(),
            new ViewTargetParser(),
        };
        ret.Add(new HelpPasrser(ret));
        return ret;
    }

    private static void AddGeneratorsToList(List<IArgumentParser> ret) => 
        ret.AddRange(QueryTypeSystemForParsers());

    private static IOrderedEnumerable<IArgumentParser> QueryTypeSystemForParsers() =>
        GeneratorFactory.AllGenerators
            .Select(i=>new PdfGenerationParser(i))
            .OrderBy(SortByCommand);

    private static string SortByCommand(IArgumentParser i) => i.Prefix;
}