using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Melville.Pdf.Model.Renderers.DocumentRenderers;
using Melville.Pdf.ReferenceDocuments.Infrastructure;
using Melville.Pdf.SkiaSharp;
using Melville.Pdf.Wpf.Rendering;
using Melville.TestHelpers.StringDatabase;
using Xunit;
namespace Melville.Wpf.IntegrationTesting;

public class RenderingTest:IClassFixture<StringTestDatabase>
{
    private readonly StringTestDatabase hashes;
    public RenderingTest(StringTestDatabase hashes)
    {
        this.hashes = hashes; 
        hashes.Recording = true;
    }

    public static IEnumerable<object[]> GeneratorTests() =>
        GeneratorFactory.AllGenerators.Select(i => new object[] { i.Prefix, i });

    [WpfTheory]
    [MemberData(nameof(GeneratorTests))]
    public async Task WpfRenderingTestAsync(string shortName, IPdfGenerator generator)
    {
        var computeWpfHashAsync = await ComputeWpfHashAsync(generator);
        hashes.AssertDatabase(computeWpfHashAsync, "wpf" + shortName);
    }

    private Task<string> ComputeWpfHashAsync(IPdfGenerator generator) =>
        ComputeGenericHashAsync(generator, AsWpfPageAsync);

    private ValueTask AsWpfPageAsync(DocumentRenderer documentRenderer, Stream target)
    {
        var rtdg = new RenderToDrawingGroup(documentRenderer, 0);
        return rtdg.RenderToPngStreamAsync(target);
    }
    
    [Theory]
    [MemberData(nameof(GeneratorTests))]
    public async Task SkiaRenderingTestAsync(string shortName, IPdfGenerator generator) => 
        hashes.AssertDatabase(await ComputeSkiaHashAsync(generator), "Skia"+shortName);

    private static Task<string> ComputeSkiaHashAsync(IPdfGenerator generator) =>
        ComputeGenericHashAsync(generator, (documentRenderer, target) => 
            RenderWithSkia.ToPngStreamAsync(documentRenderer, 0, target, -1, 1024));

    private static async Task<string> ComputeGenericHashAsync(IPdfGenerator generator,
        Func<DocumentRenderer, Stream, ValueTask> renderTo)
    {
        try
        {
            using var doc = await generator.ReadDocumentAsync();
            var target = new WriteToAdlerStream();
            await renderTo(doc, target);
            var hash = target.Computer.GetHash().ToString();
            return hash;
        }
        catch (Exception e)
        {
            var message = $"""
                {e.Message}
                {e.StackTrace}
                """;
            return message;
        }
    }
}