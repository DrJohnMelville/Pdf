using Melville.Parsing.MultiplexSources;

namespace Melville.Fonts.SfntParsers.TableDeclarations.CMaps
{
    /// <summary>
    /// This class represents a Cmap from a font file.  You can inspect the tables available and
    /// request one
    /// </summary>
    /// <param name="source">MultiplexSource containing the Cmap</param>
    /// <param name="subtables">The CmapTables used</param>
    public class ParsedCmap(IMultiplexSource source, CmapTablePointer[] subtables)
    {
        /// <summary>
        /// CMaps offered by this font
        /// </summary>
        public IReadOnlyList<CmapTablePointer> Tables => subtables;
    }
}