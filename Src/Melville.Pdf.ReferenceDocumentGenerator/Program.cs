using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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
        typeof(Program).Assembly.GetTypes()
            .Where(IsGeneratorType())
            .Select(CreateWithDefaultConstructor)
            .OrderBy(SortByCommand);

    private static Func<Type, bool> IsGeneratorType() => i => 
        i != typeof(CreatePdfParser) && i.IsAssignableTo(typeof(IPdfGenerator)) && !i.IsAbstract;

    private static IArgumentParser CreateWithDefaultConstructor(Type i) => 
        new PdfGenerationParser((IPdfGenerator)(Activator.CreateInstance(i) ?? 
                          throw new InvalidOperationException("Cannot Create: " + i)));

    private static string SortByCommand(IArgumentParser i) => i.Prefix;
}