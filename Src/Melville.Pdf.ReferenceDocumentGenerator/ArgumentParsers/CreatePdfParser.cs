namespace Melville.Pdf.ReferenceDocumentGenerator.ArgumentParsers;

public class PdfGenerationParser: IArgumentParser
{
    private readonly IPdfGenerator generator;

    public PdfGenerationParser(IPdfGenerator generator)
    {
        this.generator = generator;
    }

    public string Prefix => generator.Prefix;
    public string HelpText => generator.HelpText;

    public async ValueTask<IArgumentParser?> ParseArgumentAsync(string argument, IRootParser root)
    {
        Console.WriteLine("Generating: " + Prefix);
        await using (var targetStream = root.Target.CreateTargetStream())
        {
            await generator.WritePdfAsync(targetStream);
        }
        root.Target.View();
        return null;
        
    }
}