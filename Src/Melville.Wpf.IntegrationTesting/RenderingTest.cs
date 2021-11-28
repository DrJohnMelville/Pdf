using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Melville.Parsing.Streams;
using Melville.Pdf.LowLevel.Filters.StreamFilters;
using Melville.Pdf.LowLevel.Parsing.ParserContext;
using Melville.Pdf.Model.Documents;
using Melville.Pdf.ReferenceDocuments.Infrastructure;
using Melville.Pdf.SkiaSharp;
using Melville.Pdf.Wpf;
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

    private static IEnumerable<object[]> GeneratorTests() =>
        GeneratorFactory.AllGenerators.Select(i => new object[] { i.Prefix, i });

    [WpfTheory]                                                         
    [MemberData(nameof(GeneratorTests))]
    public async Task WpfRenderingTest(string shortName, IPdfGenerator generator) =>
        hashes.AssertDatabase(await ComputeWpfHash(generator), "wpf" + shortName);
    
    private Task<string> ComputeWpfHash(IPdfGenerator generator) =>
        ComputeGenericHash(generator, HasWpfPage);

    private ValueTask HasWpfPage(PdfPage page, Stream target)
    {
        return RenderToDrawingGroup.RenderToPngStream(page, target);
    }

    [Theory]
    [MemberData(nameof(GeneratorTests))]
    public async Task SkiaRenderingTest(string shortName, IPdfGenerator generator) => 
        hashes.AssertDatabase(await ComputeSkiaHash(generator), "Skia"+shortName);

    private static Task<string> ComputeSkiaHash(IPdfGenerator generator) =>
        ComputeGenericHash(generator, (page, target) => RenderWithSkia.ToPngStream(page, target, -1, 1024));
    private static async Task<string> ComputeGenericHash(IPdfGenerator generator,
        Func<PdfPage, Stream, ValueTask> renderTo)
    {
        try
        {
            var doc = await ReadDocument(generator);
            var target = new WriteToAdlerStream();
            var firstPage = await (await doc.PagesAsync()).GetPageAsync(0);
            await renderTo(firstPage, target);
            return target.Computer.GetHash().ToString();
        }
        catch (Exception e)
        {
            return e.Message;
        }
    }

    private static async Task<PdfDocument> ReadDocument(IPdfGenerator generator)
    {
        MultiBufferStream src = new();
        await generator.WritePdfAsync(src);
        var doc = await PdfDocument.ReadAsync(
            src.CreateReader(), new ConstantPasswordSource(PasswordType.User, generator.Password));
        return doc;
    }
}