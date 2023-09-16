using System.Diagnostics;
using System.Security.Cryptography.X509Certificates;
using CommandLine;
using CommandLine.Text;
using Melville.INPC;
using Melville.Pdf.Model.Renderers.DocumentRenderers;
using Melville.Pdf.SkiaSharp;

namespace TimeTrial;


internal class Program
{
    static async Task Main(string[] args)
    {
        await Parser.Default.ParseArguments<Options>(args).MapResult(
            MainWithzOption, ParseError);
    }

    private static Task ParseError(IEnumerable<Error> arg) => Task.CompletedTask;

    private static async Task MainWithzOption(Options arg)
    {
        Console.WriteLine("Name,Page,Seconds");
        await foreach (var item in arg.Items())
        {
            Console.WriteLine(await item.RunTestAsync());

        }
    }
}

public record struct SourceItem(string Name, DocumentRenderer Renderer, int Page)
{
    public SourceItem(string name, DocumentRenderer renderer) : this(name, renderer, -1){}

    public async ValueTask<string> RunTestAsync()
    {
        var stopwatch = new Stopwatch();
        stopwatch.Start();
        await RenderWithSkia.ToSurfaceAsync(Renderer, Page, 800);
        stopwatch.Stop();
        return $"""
                "{Name}", {Page}, {stopwatch.Elapsed.TotalSeconds:######0.0000}
                """;
    }
}