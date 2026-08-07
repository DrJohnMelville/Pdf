using System.Numerics;

namespace Melville.Fonts.SfntParsers.TableDeclarations.CffGlyphs;

internal partial class CffInstructionExecutor<T> : IDisposable where T : ICffGlyphTarget
{
    private VectorBuffer flexPoints;
    private int flexPosition = 0;

    /// <summary>
    /// OtherSubrs are a special case of subroutines that are used for the flex operator.  
    /// Once we are within a flex operator the code uses rmoves to move the current point, which then gets
    /// updated with subsequent othersubr calls.  Thuse we should not pass those rmoves on to the target,
    /// because they wiull be handled by the flex operator.  This class wraps a target and filters out those rmoves.            
    /// </summary>
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
                StackSize--;
                flexPosition = 0;
                break;
            case 1:
                flexPoints[0] = this.IncrementCurrentPoint(0, 0);
                flexPosition = 1;
                StackSize--;
                break;
            case 2:
                flexPoints[flexPosition++] = this.IncrementCurrentPoint(0, 0);
                StackSize--;
                break;
            case 3:
                StackSize--;
                break;
        }
        return ValueTask.FromResult(0);
    }

    private ValueTask<int> DoPopAsync()
    {
        StackSize--;
         return ValueTask.FromResult(0);
    }

    private void DoSetCurrentPoint()
    {
        // this is supposed to set the curent poing after a otherSubr flex call, but it works as a no-op in the
        // fonts I have seen, and it does not work when I try to do what I think it should do,
        // so I am leaving it as a no-op for now.

        //var span = CurrentStackSpan;
        //CurrentX = span[^1].FloatValue;
        //CurrentY = span[^2].FloatValue;
        StackSize -= 2;
    }


    [System.Runtime.CompilerServices.InlineArray(8)]
    public struct VectorBuffer
    {
        private Vector2 _element0;
    }
}

