using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Melville.Parsing.Streams;
using Melville.Pdf.LowLevel.Parsing.ParserContext;
using Melville.Pdf.Model;
using Melville.Pdf.Model.Documents;
using Melville.Pdf.Model.Renderers;
using Melville.Pdf.ReferenceDocuments.Infrastructure;
using Melville.Pdf.SkiaSharp;
using Melville.Pdf.Wpf.Rendering;
using Melville.TestHelpers.StringDatabase;
using Xunit;
namespace Melville.Wpf.IntegrationTesting;

public class RenderingTest: IClassFixture<StringTestDatabase>
{
    private readonly StringTestDatabase hashes;

    public RenderingTest(StringTestDatabase hashes)
    {
        this.hashes = hashes; 
        //hashes.Recording = true;
    }

    public static IEnumerable<object[]> GeneratorTests() =>
        GeneratorFactory.AllGenerators.Select(i => new object[] { i.Prefix, i });

    [WpfTheory]                                                         
    [MemberData(nameof(GeneratorTests))]
    public async Task WpfRenderingTest(string shortName, IPdfGenerator generator) =>
        hashes.AssertDatabase(await ComputeWpfHash(generator), "wpf" + shortName);
    
    private Task<string> ComputeWpfHash(IPdfGenerator generator) =>
        ComputeGenericHash(generator, AsWpfPage);

    private ValueTask AsWpfPage(DocumentRenderer documentRenderer, Stream target)
    {
        var rtdg = new RenderToDrawingGroup(documentRenderer, 0);
        return rtdg.RenderToPngStream(target);
    }

    [Theory]
    [MemberData(nameof(GeneratorTests))]
    public async Task SkiaRenderingTest(string shortName, IPdfGenerator generator) => 
        hashes.AssertDatabase(await ComputeSkiaHash(generator), "Skia"+shortName);

    private static Task<string> ComputeSkiaHash(IPdfGenerator generator) =>
        ComputeGenericHash(generator, (documentRenderer, target) => 
            RenderWithSkia.ToPngStreamAsync(documentRenderer, 0, target, -1, 1024));

    private static async Task<string> ComputeGenericHash(IPdfGenerator generator,
        Func<DocumentRenderer, Stream, ValueTask> renderTo)
    {
        try
        {
            var doc = await RenderTestHelpers.ReadDocument(generator);
            var target = new WriteToAdlerStream();
            await renderTo(doc, target);
            return target.Computer.GetHash().ToString();
        }
        catch (Exception e)
        {
            return e.Message;
        }
    }
}

public static class RenderTestHelpers
{
    public static async ValueTask<DocumentRenderer> ReadDocument(IPdfGenerator generator)
    {
        MultiBufferStream src = new();
        await generator.WritePdfAsync(src);
        return await new PdfReader(new ConstantPasswordSource(PasswordType.User, generator.Password))
            .ReadFrom(src.CreateReader());
    }
}