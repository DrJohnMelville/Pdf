using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Melville.Fonts;
using Melville.Fonts.SfntParsers.TableDeclarations.Heads;
using Melville.Parsing.AwaitConfiguration;
using Melville.Parsing.MultiplexSources;
using Melville.Pdf.Model.Renderers.FontRenderings.FontLibraries;
using Melville.SharpFont;

namespace Melville.Pdf.Model.Renderers.FontRenderings.FreeType.FontLibraries;

internal readonly struct FontLibraryBuilder
{
    private readonly Dictionary<string, FontFamily> fonts = new( );

    public FontLibraryBuilder()
    {
    }

    public async ValueTask<FontLibrary> BuildFromAsync(string fontFolder)
    {
        foreach (var fontFile in Directory.EnumerateFiles(fontFolder))
        {
            await using var file = File.OpenRead(fontFile);
            var fonts = 
                await RootFontParser.ParseAsync(MultiplexSourceFactory.Create(file)).CA();
            int index = 0;
            foreach (var font in fonts)
            {
                var reference = new FontReference(fontFile, index++);
                var family = GetOrCreateFamily(await font.FontFamilyNameAsync().CA());
                var style = await font.GetFontStyleAsync().CA();
                family.Register(reference, style.HasFlag(MacStyles.Bold), style.HasFlag(MacStyles.Italic));
            }
        }

        return new FontLibrary(fonts.Values.ToArray());
    }

    private FontFamily GetOrCreateFamily(string family)
    {
        if (!fonts.TryGetValue(family, out var found))
        {
            found = new FontFamily(family);
            fonts.Add(family, found);
        }
        return found;
    }
}