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
        @"C:\Users\jom252\source\repos\DrJohnMelville\Pdf\Src\Melville.Pdf.ReferenceDocuments\Text\Zev.ttf");

    private IGenericFont freeType = new FreeTypeFace(NewMemoryFace());

    private Lazy<Task<IGenericFont>> mt = new (
        async() => (await RootFontParser.ParseAsync(MultiplexSourceFactory.Create(data)))[0]);

    private static Face NewMemoryFace()
    {
        var newMemoryFace = GlobalFreeTypeResources.SharpFontLibrary.NewMemoryFace(data, 0);
        newMemoryFace.SetCharSize(0, 65, 0, 0);
        return newMemoryFace;
    }

    private static readonly IGlyphTarget target = new NullTarget();

    [Benchmark]
    public Task FreeType() => Render(freeType);
    [Benchmark(Baseline = true)]
    public async Task Melville() => Render(await mt.Value);

    private static async Task Render(IGenericFont genericFont)
    {
        var glyphSourceAsync = await genericFont.GetGlyphSourceAsync();
        for (uint i = 0; i < glyphSourceAsync.GlyphCount; i++)
        {
            await glyphSourceAsync
                .RenderGlyphAsync(i, target, Matrix3x2.Identity);

        }
    }
}

public class NullTarget : IGlyphTarget
{
    public void MoveTo(Vector2 point)
    {
    }

    public void LineTo(Vector2 point)
    {
    }

    public void CurveTo(Vector2 control, Vector2 endPoint)
    {
    }

    public void CurveTo(Vector2 control1, Vector2 control2, Vector2 endPoint)
    {
    }

    public void EndGlyph()
    {
    }
}