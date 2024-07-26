using System.Numerics;

namespace Melville.Fonts.SfntParsers.TableDeclarations.CffGlyphs;

internal partial class CffInstructionExecutor<T> : IDisposable where T : ICffGlyphTarget
{
    private VectorBuffer flexPoints;
    private int flexPosition = 0;
    private bool ShouldSuppressRmove => flexPosition > 0;

    private ValueTask<int> DoOtherSubrAsync()
    {
        switch (CurrentStackSpan[^1].IntValue)
        {
            case 0: FlexImplementation(
                flexPoints[0],
                flexPoints[2],
                flexPoints[3],
                flexPoints[4],
                flexPoints[5],
                flexPoints[6],
                flexPoints[7],
                0 // always draw the two curves
                );
                StackSize = 0;
                flexPosition = 0;
                break;
            case 1:
                flexPoints[0] = this.IncrementCurrentPoint(0, 0);
                flexPosition = 1;
                StackSize = 0;
                break;
            case 2:
                flexPoints[flexPosition++] = this.IncrementCurrentPoint(0, 0);
                StackSize = 0;
                break;
            case 3:
                Stack[0] = new DictValue(3);
                StackSize = 1;
                break;
        }
        return ValueTask.FromResult(0);
    }

    private ValueTask<int> DoPopAsync() => ValueTask.FromResult(0);


    [System.Runtime.CompilerServices.InlineArray(8)]
    public struct VectorBuffer
    {
        private Vector2 _element0;
    }
}

