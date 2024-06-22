using System.Diagnostics;
using Melville.Fonts.SfntParsers.TableDeclarations.CffGlyphs;

namespace Melville.Fonts.SfntParsers.TableDeclarations.CFF2Glyphs
{
    internal abstract class RootFontDictSelector: 
        IFontDictSelector, IFontDictExecutorSelector
    {
        private IGlyphSubroutineExecutor[] indexes = [];
        public IFontDictExecutorSelector GetSelector(Span<IGlyphSubroutineExecutor> indexes)
        {
            Debug.Assert(this.indexes.Length is 0);
            if (indexes.Length is 1) return indexes[0];
            this.indexes = indexes.ToArray();
            return this;
        }

        public IGlyphSubroutineExecutor GetExecutor(uint glyph) =>
            indexes[SelectIndexFor(glyph)];
        protected abstract int SelectIndexFor(uint glyph);
    }
}