namespace Melville.Fonts.SfntParsers.TableDeclarations.TrueTypeGlyphs
{
    internal interface IRecorderAllocator
    {
        CapturedPoint[] Allocate(int size);
        void Free(CapturedPoint[] data);
    }
}