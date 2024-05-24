using System.IO;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Melville.Fonts;
using Melville.Fonts.SfntParsers.TableDeclarations.CMaps;
using Melville.Parsing.MultiplexSources;

namespace Melville.Pdf.DataModelTests.Fonts.Integration
{
    public static class IntegrationFontLoader
    {
        public static async Task<IGenericFont> LoadAsync([CallerFilePath] string path = null!)
        {
            var src = MultiplexSourceFactory.Create(
                File.Open(FontFileName(path), FileMode.Open, FileAccess.Read, FileShare.Read));
            var font = (await RootFontParser.ParseAsync(src))[0];
            return font;
        }
        private static string FontFileName(string path) => path.Replace(".cs", ".ttf");

        public static async ValueTask<ICmapImplementation?> CmapByIndexAsync(
            this IGenericFont font, int index) =>
            await (await font.ParseCMapsAsync()).GetByIndexAsync(index);


    }
}