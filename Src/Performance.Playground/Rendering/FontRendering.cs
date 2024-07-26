using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using Melville.Fonts;
using Melville.Parsing.MultiplexSources;
using Melville.Pdf.Model.Renderers.FontRenderings.FreeType;
using Melville.SharpFont;

namespace Performance.Playground.Rendering;

[MemoryDiagnoser]
[InProcess]
public class FontRendering
{
    private static readonly Byte[] data = File.ReadAllBytes(
        @"C:\Users\jmelv\source\repos\DrJohnMelville\Pdf\Src\Melville.Pdf.ReferenceDocuments\Text\GFSEustace.otf");

    private IGenericFont freeType = new FreeTypeFace(NewMemoryFace());

    private Lazy<Task<IGenericFont>> mt = new (
        async() => (await RootFontParser.ParseAsync(MultiplexSourceFactory.Create(data)))[0]);

    private static Face NewMemoryFace()
    {
        var newMemoryFace = GlobalFreeTypeResources.SharpFontLibrary.NewMemoryFace(data, 0);
        newMemoryFace.SetCharSize(0, 65, 0, 0);
        return newMemoryFace;
    }

    [Benchmark]
    public Task FreeTypeAsync() => RenderAsync(freeType);
    [Benchmark(Baseline = true)]
    public async Task MelvilleAsync() => await RenderAsync(await mt.Value);

    private static async Task RenderAsync(IGenericFont genericFont)
    {
        var glyphSourceAsync = await genericFont.GetGlyphSourceAsync();
        for (uint i = 0; i < glyphSourceAsync.GlyphCount; i++)
        {
            await glyphSourceAsync
                .RenderGlyphAsync(i, (IGlyphTarget)NullTarget.Instance, Matrix3x2.Identity);

        }
    }
}
