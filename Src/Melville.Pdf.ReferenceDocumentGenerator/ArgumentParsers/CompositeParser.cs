using System.Collections.Generic;

namespace Melville.Pdf.ReferenceDocumentGenerator.ArgumentParsers;

public class CompositeParser: IArgumentParser
{
    private readonly IReadOnlyCollection<IArgumentParser> parsers;
    public CompositeParser(IReadOnlyCollection<IArgumentParser> parsers)
    {
        this.parsers = parsers;
    }

    public string Prefix => "";
    public string HelpText => "";

    public ValueTask<IArgumentParser?> ParseArgumentAsync(string argument, IRootParser root)
    {
        foreach (var parser in parsers) 
        {
            if (argument.Equals(parser.Prefix, StringComparison.OrdinalIgnoreCase))
            {
                return parser.ParseArgumentAsync(argument, root);
            }
        }
        Console.WriteLine($"Unknown command: {argument}");
        return ValueTask.FromResult((IArgumentParser?)null);
    }
}