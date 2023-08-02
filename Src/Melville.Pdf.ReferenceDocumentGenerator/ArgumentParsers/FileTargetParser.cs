using Melville.Pdf.ReferenceDocumentGenerator.Targets;

namespace Melville.Pdf.ReferenceDocumentGenerator.ArgumentParsers;

public class FileTargetParser: IArgumentParser
{
    public string Prefix => "-f";
    public string HelpText => "<file> output to a specific file name.";

    public ValueTask<IArgumentParser?> ParseArgumentAsync(string argument, IRootParser root)
    {
        return ValueTask.FromResult<IArgumentParser?>(new FileTargetCompletion());
    }
}

public class FileTargetCompletion : IArgumentParser
{
    public string Prefix => "";
    public string HelpText => "";

    public ValueTask<IArgumentParser?> ParseArgumentAsync(string argument, IRootParser root)
    {
        root.Target = new FileTarget(argument);
        return new ValueTask<IArgumentParser?>();
    }
}