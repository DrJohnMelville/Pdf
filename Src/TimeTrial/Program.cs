using System.Diagnostics;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks.Sources;
using CommandLine;
using CommandLine.Text;
using Melville.INPC;
using Melville.Parsing.LinkedLists;
using Melville.Parsing.ObjectRentals;
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
        var stopWatch = new Stopwatch();
        stopWatch.Start();
        await foreach (var item in arg.Items())
        {
           var line = await item.RunTestAsync();
            Console.WriteLine(line);
        }
        stopWatch.Stop();
        Console.WriteLine($"Total Time: {stopWatch.Elapsed.TotalSeconds:#######0.0000}");
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