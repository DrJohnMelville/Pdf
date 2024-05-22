using System.Runtime.CompilerServices;
using Melville.Fonts;
using Melville.Fonts.SfntParsers.TableDeclarations.CMaps;
using Melville.INPC;

namespace Melville.Pdf.LowLevelViewerParts.FontViewers.CmapViewers;

[AutoNotify]
public partial class CMapViewModel (ICMapSource cmap, int index)
{
    public string Title
    {
        get
        {
            var (platform, encoding) = cmap.GetPlatformEncoding(index);
            return $"{platform}:{encoding}";
        }
    }

    private readonly LoadOnce<IReadOnlyList<string>> mappings = new(["Loading Mapping"]);
    public IReadOnlyList<string> Mappings => mappings.GetValue(this, LoadMappings);

    private async ValueTask<IReadOnlyList<string>> LoadMappings()
    {
        try
        {
            var subTable = await cmap.GetByIndexAsync(index);
            return subTable.AllMappings()
                .Select(i => $"{VariableByteString(i.Bytes, i.Character)} => {i.Glyph:X4}").ToArray();
        }
        catch (Exception e)
        {
            return [e.Message];
        }
    }

    private string VariableByteString(int bytes, uint character) => bytes switch
    {
        1 => $"{character:X2}",
        2 => $"{character:X4}",
        4 => $"{character:X8}",
        _ => $"{character:X}"
    };
 }