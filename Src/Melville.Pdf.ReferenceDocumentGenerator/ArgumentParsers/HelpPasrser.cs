using System.Collections.Generic;
using System.Linq;

namespace Melville.Pdf.ReferenceDocumentGenerator.ArgumentParsers;

public class HelpPasrser: IArgumentParser
{
    private IReadOnlyList<IArgumentParser> parsers;
    private int PrefixLength => parsers.Max(i => i.Prefix.Length);
        
    public HelpPasrser(IReadOnlyList<IArgumentParser> parsers)
    {
        this.parsers = parsers;
    }

    public string Prefix => "-h";
    public string HelpText => "Display this help information";

    public ValueTask<IArgumentParser?> ParseArgumentAsync(string argument, IRootParser root)
    {
        WriteHeader();
        WriteCommandHelp();
        return ValueTask.FromResult((IArgumentParser?) null);
    }

    private static void WriteHeader()
    {
        Console.WriteLine();
        Console.WriteLine("Help");
        Console.WriteLine("----");
    }

    private void WriteCommandHelp()
    {
        var formatString = FormatString();
        foreach (var parser in parsers)
        {
            Console.WriteLine(TextForParser(parser, formatString));
        }
    }

    private string TextForParser(IArgumentParser parser, string formatString) => 
        string.Format(formatString, parser.Prefix, parser.HelpText);

    private string FormatString() => "  {0," + PrefixLength + "}  {1}";
}