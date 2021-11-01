using System;
using System.IO;
using System.Threading.Tasks;
using Melville.Pdf.LowLevel.Filters.StreamFilters;

namespace Melville.Pdf.ReferenceDocumentGenerator.ArgumentParsers;

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

public static class CreatePdfParserOperations
{
    public static async ValueTask<MultiBufferStream> AsMultiBuf(this CreatePdfParser source)
    {
        var target = new MultiBufferStream();
        await source.WritePdfAsync(target);
        return target;
    }
    public static async ValueTask<string> AsString(this CreatePdfParser source) => 
        await new StreamReader((await source.AsMultiBuf()).CreateReader()).ReadToEndAsync();
}