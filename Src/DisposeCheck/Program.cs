// See https://aka.ms/new-console-template for more information

using Melville.Parsing.ObjectRentals;
using Melville.Pdf.ReferenceDocuments.Infrastructure;
using Melville.Pdf.SkiaSharp;

foreach (var generator in GeneratorFactory.AllGenerators)
{
    if (!generator.Prefix.StartsWith("-JBigSampleBitStream1")) continue;
    Console.WriteLine(generator.Prefix);
    {
        using var ctx = RentalPolicyChecker.RentalScope(Console.WriteLine);
        using var renderer = await generator.ReadDocumentAsync();
        await RenderWithSkia.ToSurfaceAsync(renderer, 1, 800);
    }
    Console.ReadLine();
}

Console.WriteLine("Done!");
