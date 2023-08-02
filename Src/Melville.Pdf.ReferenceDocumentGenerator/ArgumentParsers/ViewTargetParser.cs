using Melville.Pdf.ReferenceDocumentGenerator.Targets;

namespace Melville.Pdf.ReferenceDocumentGenerator.ArgumentParsers;

public class ViewTargetParser: IArgumentParser
{
    public string Prefix => "-v";
    public string HelpText => "Display the generated file immediately.";
        
    public ValueTask<IArgumentParser?> ParseArgumentAsync(string argument, IRootParser root)
    {
        root.Target = new FileTarget(argument);
        return new ValueTask<IArgumentParser?>();
    }
}