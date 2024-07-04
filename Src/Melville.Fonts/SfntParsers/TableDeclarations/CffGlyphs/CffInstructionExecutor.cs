using System.Buffers;
using System.Diagnostics;
using System.Numerics;
using Melville.INPC;
using Melville.Parsing.AwaitConfiguration;
using Melville.Parsing.SpanAndMemory;

namespace Melville.Fonts.SfntParsers.TableDeclarations.CffGlyphs;

#warning this class needs allocation free implementation
internal partial class CffInstructionExecutor<T>: IDisposable where T:ICffGlyphTarget
{
    #region Variables Creation and Destruction

    // per Adobe Technical Note #5177, page 33 this should be 48, but
    // I found values over 512 in real fonts.
    private const int MaximumCffInstructionOperands = 1024;
    private const int MamimumTransientArraySize = 32;

    [FromConstructor] private readonly T target;
    [FromConstructor] private readonly Matrix3x2 transform;
    [FromConstructor] private readonly IGlyphSubroutineExecutor globalSuboutines;
    [FromConstructor] private readonly IGlyphSubroutineExecutor localSubroutines;
    [FromConstructor] private readonly uint[] variatons;
    private int currentVariation;
    private DictValue[] Stack { get;} = ArrayPool<DictValue>.Shared.Rent(MaximumCffInstructionOperands);
    private readonly float[] storedValues = ArrayPool<float>.Shared.Rent(MamimumTransientArraySize);

    partial void OnConstructed()
    {
        StackSize = 0;
        CurrentX = CurrentY = HintCount = 0;
    }

    private Span<DictValue> CurrentStackSpan => Stack.AsSpan(0, StackSize);

    public void Dispose()
    {
        ArrayPool<DictValue>.Shared.Return(Stack);
        ArrayPool<float>.Shared.Return(storedValues);
    }

    #endregion

    #region Intstruction Sequence Execution

    public async ValueTask ExecuteInstructionSequenceAsync(ReadOnlySequence<byte> sourceSequence)    {
        try
        {
            while (ReadInstruction(ref sourceSequence, out var instruction))
            {
                var skipBytes =await ExecuteInstructionAsync(
                    (CharStringOperators)instruction).CA();
                switch (skipBytes)
                {
                    case <0: return;
                    case >0: 
                        sourceSequence = sourceSequence.Slice(skipBytes);
                        break;
                }
            }
        }
        catch (Exception )
        {
        }
    }

    private bool ReadInstruction(ref ReadOnlySequence<byte> source, out int instruction)
    {
        var parser = new DictParser<CharString2Definition>(new SequenceReader<byte>(source), Stack);
        instruction = parser.ReadNextInstruction(StackSize);
        StackSize = parser.OperandPosition;
        source = parser.UnreadSequence;
        return instruction != 255;
    }


    #endregion

    #region InstructionDecoding

    private int stackSize;
    public ref int StackSize => ref stackSize;

    public float CurrentX {get; private set;}
    public float CurrentY {get; private set; }
    public int HintCount { get; private set; }


    private ValueTask<int> ExecuteInstructionAsync(CharStringOperators instruction)
    {
#if DEBUG
        target.Operator(instruction, CurrentStackSpan);
#endif
        switch (instruction)
        {
            case CharStringOperators.CallSubr: return CallSubrAsync(localSubroutines);
            case CharStringOperators.CallGSubr: return CallSubrAsync(globalSuboutines);
            case CharStringOperators.IfElse: return DoIfAsync();
            case CharStringOperators.Eq: return FuncAsync((i,j)=>i==j?1f:0f);
            case CharStringOperators.Not: return DoNotAsync();
            case CharStringOperators.And: return BoolFuncAsync((i,j)=>i&&j);
            case CharStringOperators.Or: return BoolFuncAsync((i,j)=>i||j);
            case CharStringOperators.Get: return FuncAsync(i=>storedValues[(int)i]);
            case CharStringOperators.Put: return DoPutAsync();
            case CharStringOperators.Dup: return PushAsync(CurrentStackSpan[^1].FloatValue);
            case CharStringOperators.Roll: return DoRollAsync();
            case CharStringOperators.Exch: return DoExchangeAsync();
            case CharStringOperators.Drop: return DoDropAsync();
            case CharStringOperators.Random: return PushAsync((float)RandomSource.NextDouble());
            case CharStringOperators.Add: return FuncAsync((i, j) => i + j);
            case CharStringOperators.Sub: return FuncAsync((i, j) => i - j);
            case CharStringOperators.Mul: return FuncAsync((i, j) => i * j);
            case CharStringOperators.Div: return FuncAsync((i, j) => i / j);
            case CharStringOperators.Negative: return FuncAsync((i) => -i);
            case CharStringOperators.Abs: return FuncAsync(Math.Abs);
            case CharStringOperators.Sqrt: return FuncAsync(i=>(float)Math.Sqrt(i));
            case CharStringOperators.HintMask: return DoHintMaskAsync();
            case CharStringOperators.CntrMask: return DoHintMaskAsync();
            case CharStringOperators.HStem: DoHintWithWidth(); break;
            case CharStringOperators.HStemHM: DoHintWithWidth(); break;
            case CharStringOperators.VStem: DoHintWithWidth(); break;
            case CharStringOperators.VStemHM: DoHintWithWidth(); break;
            case CharStringOperators.RMoveTo: DoRmoveTo(ReportWidthIf(StackSize>2)); break;
            case CharStringOperators.HMoveTo: DoHMoveTo(ReportWidthIf(StackSize>1)); break;
            case CharStringOperators.VMoveTo: DoVMoveTo(ReportWidthIf(StackSize>1)); break;
            case CharStringOperators.RLineTo: DoRLineTo(CurrentStackSpan); break;
            case CharStringOperators.HLineTo: DoHLineTo(); break;
            case CharStringOperators.VLineTo: DoVLineTo(); break;
            case CharStringOperators.RRCurveTo: DoRRCurveTo(CurrentStackSpan); break;
            case CharStringOperators.HHCurveTo: DoHHCurveTo(); break;
            case CharStringOperators.VVCurveTo: DoVVCurveTo(); break;
            case CharStringOperators.HVCurveTo: DoHVCurveTo(); break;
            case CharStringOperators.VHCurveTo: DoVHCurveTo(); break;
            case CharStringOperators.RCurveLine: DoRCurveLine(); break;
            case CharStringOperators.RLineCurve: DoRLineCurve(); break;
            case CharStringOperators.Flex: DoFlex(); break;
            case CharStringOperators.HFlex: DoHFlex(); break;
            case CharStringOperators.HFlex1: DoHFlex1(); break;
            case CharStringOperators.Flex1: DoFlex1(); break;
            case CharStringOperators.EndChar: return SendEndGlyphAsync();
            case CharStringOperators.Return: return ReturnFromSubroutineAsync();
            case CharStringOperators.Blend: return BlendAsync();
            case CharStringOperators.VsIndex: VsIndex(); break;
            //type 1 specific operators
            case CharStringOperators.ClosePath: break; // all paths are closed.
            case CharStringOperators.HStem3:  break;
            case CharStringOperators.Seac:  break; 
            case CharStringOperators.SbW:  break;
            case CharStringOperators.Hsbw:  break;
            case CharStringOperators.CallOtherSubr:  break;
            case CharStringOperators.Pop:  break;
            case CharStringOperators.SetCurrentPoint:  break;
            default:
                throw new NotSupportedException($"Charstring Operator {instruction} is not implemented ");
        }
        StackSize = 0;
        return ValueTask.FromResult(0);
    }


    #endregion

    #region Blending

    private void VsIndex()
    {
        currentVariation = Math.Min(CurrentStackSpan[^1].IntValue, variatons.Length - 1);
    }
    
    private ValueTask<int> BlendAsync()
    {
        var n = CurrentStackSpan[^1].IntValue;
        var delta = variatons[currentVariation];
        var totalDelta = (int)(delta * n) + 1;
        if (StackSize < (n + totalDelta)) return ValueTask.FromResult(0);
        StackSize -= totalDelta;
        Debug.Assert(StackSize >= n );
        return ValueTask.FromResult(0);
    }

    #endregion

    #region Language Elements


    private async ValueTask<int> CallSubrAsync(
        IGlyphSubroutineExecutor glyphSubroutineExecutor)
    {
        StackSize--;
        await glyphSubroutineExecutor.CallAsync(Stack[StackSize].IntValue, 
            ExecuteInstructionSequenceAsync).CA();
        return 0;
    }

    private ValueTask<int> DoIfAsync()
    {
        var span = CurrentStackSpan;
        var decision = span[^2].FloatValue <= span[^1].FloatValue;
        if (!decision)
            span[^4] = span[^3];
        StackSize -= 3;
        return ValueTask.FromResult(0);
    }


    private ValueTask<int> BoolFuncAsync(Func<bool, bool, bool> op)
    {
        var span = CurrentStackSpan;
        span[^2] = new DictValue(op(span[^2].IntValue!=0, span[^1].IntValue!=0)?1:0);
        StackSize--;
        return ValueTask.FromResult(0);
    }

    private ValueTask<int> DoPutAsync()
    {
        StackSize -= 2;
        storedValues[(int)Stack[StackSize+1].IntValue] = Stack[StackSize].FloatValue;
        return ValueTask.FromResult(0);
    }

    private ValueTask<int> DoRollAsync()
    {
        StackSize -= 2;
        CurrentStackSpan.Roll(Stack[StackSize+1].IntValue, Stack[StackSize].IntValue );
        return ValueTask.FromResult(0);
    }

    private ValueTask<int> DoExchangeAsync()
    {
        var span = CurrentStackSpan;
        (span[^1], span[^2]) = (span[^2], span[^1]);
        return ValueTask.FromResult(0);
    }

    private ValueTask<int> DoDropAsync()
    {
        StackSize--;
        return ValueTask.FromResult(0);
    }

    #endregion

    #region Math operations

    private ValueTask<int> DoNotAsync()
    {
        var span = CurrentStackSpan;
        span[^1] = new DictValue(span[^1].IntValue == 0 ? 1 : 0);
        return ValueTask.FromResult(0);
    }
    public static readonly Random RandomSource = new();

    private ValueTask<int> PushAsync(float value)
    {
        Stack[StackSize++] = new DictValue(value);
        return ValueTask.FromResult(0);
    }

    private ValueTask<int> FuncAsync(Func<float, float, float> op)
    {
        var span = CurrentStackSpan;
        span[^2] = new DictValue(op(span[^2].FloatValue, span[^1].FloatValue));
        StackSize--;
        return ValueTask.FromResult(0);
    }

    private ValueTask<int> FuncAsync(Func<float, float> op)
    {
        var span = CurrentStackSpan;
        span[^1] = new DictValue(op(span[^1].FloatValue));
        return ValueTask.FromResult(0);
    }

    #endregion

    #region Hints

    private ValueTask<int> DoHintMaskAsync()
    {
        DoHintWithWidth();
        StackSize = 0;
        return ValueTask.FromResult((HintCount + 7) / 8);
    }

    private void DoHintWithWidth()
    {
        if (StackSize % 2 == 1) target.RelativeCharWidth(Stack[0].FloatValue);
        HintCount += StackSize / 2;
    }

    private Span<DictValue> ReportWidthIf(bool cond)
    {
        if (cond)
        {
            target.RelativeCharWidth(Stack[0].FloatValue);
            return CurrentStackSpan[1..];
        }
        return CurrentStackSpan;
    }

    #endregion

    #region Path creation operations

    private void DoRmoveTo(Span<DictValue> point) => 
        target.MoveTo(IncrementCurrentPoint(point[0].FloatValue, point[1].FloatValue));

    private void DoHMoveTo(Span<DictValue> coord) => 
        target.MoveTo(IncrementCurrentPoint(coord[0].FloatValue, 0));

    private void DoVMoveTo(Span<DictValue> coord) => 
        target.MoveTo(IncrementCurrentPoint(0, coord[0].FloatValue));

    private void DoRLineTo(Span<DictValue> span)
    {
        for (int i = 0; i < span.Length; i+= 2)
            target.LineTo(IncrementCurrentPointFrom(span, i));
    }

    private void DoHLineTo() => AlternatingLines(true);
    private void DoVLineTo() => AlternatingLines(false);

    private void AlternatingLines(bool horizontal)
    {
        foreach (var offset in CurrentStackSpan)
        {
            target.LineTo(horizontal
                ? IncrementCurrentPoint(offset.IntValue, 0)
                : IncrementCurrentPoint(0, offset.IntValue));
            horizontal = !horizontal;
        }
    }


    private void DoRRCurveTo(Span<DictValue> source)
    {
        while (source.Length > 5)
        {
            target.CurveTo(IncrementCurrentPointFrom(source, 0), 
                IncrementCurrentPointFrom(source, 2),
                IncrementCurrentPointFrom(source, 4));
            source = source[6..];
        }
   }

    private void DoHHCurveTo()
    {
        int i = 0;
        if (StackSize % 4 == 1)
        {
            DrawHorizontalCurve(1, Stack[0].FloatValue);
            i = 5;
        }
        for (;i < StackSize; i+=4)
        {
            DrawHorizontalCurve(i);
        }
    }

    private void DoVVCurveTo()
    {
        int i = 0;
        if (StackSize % 4 == 1)
        {
            DrawVerticalCurve(1, Stack[0].FloatValue);
            i = 5;
        }
        for (;i < StackSize; i+=4)
        {
            DrawVerticalCurve(i);
        }
    }


    private void DrawHorizontalCurve(int index, float beginY = 0)
    {
        var start = IncrementCurrentPoint(0, 0);
        var c1 = IncrementCurrentPoint(
            Stack[index].FloatValue, beginY);
        var c2 = IncrementCurrentPointFrom(index + 1);
        var final = IncrementCurrentPoint(Stack[index + 3].FloatValue, 0);
        target.CurveTo(c1, c2, final);
    }

    private void DrawVerticalCurve(int index, float beginX = 0) =>
        target.CurveTo(IncrementCurrentPoint(beginX, Stack[index].FloatValue),
            IncrementCurrentPointFrom(index+1),
            IncrementCurrentPoint(0,Stack[index+3].FloatValue));

    private void DoHVCurveTo() => AlternatingHVCurves(true);
    private void DoVHCurveTo() => AlternatingHVCurves(false);

    private void AlternatingHVCurves(bool horizontal)
    {
        for (int i = 0; i+3 < StackSize; i+=4)
        {
            var finalOffset = CheckForAdditionalTrailingOffset(i);
            if (horizontal)
                DrawHorizontalToVerticalkCurve(i, finalOffset);
            else
                DrawVerticalToHorizontalCurve(i, finalOffset);
            horizontal = !horizontal;
        }
    }

    private void DrawHorizontalToVerticalkCurve(int i, float finalOffset)
    {
        target.CurveTo(IncrementCurrentPoint(Stack[i].FloatValue, 0),
            IncrementCurrentPointFrom(i+1),
            IncrementCurrentPoint(finalOffset, Stack[i+3].FloatValue));
    }

    private void DrawVerticalToHorizontalCurve(int i, float finalOffset)
    {
        target.CurveTo(IncrementCurrentPoint(0, Stack[i].FloatValue),
            IncrementCurrentPointFrom(i+1),
            IncrementCurrentPoint(Stack[i+3].FloatValue, finalOffset));
    }

    private float CheckForAdditionalTrailingOffset(int i)
    {
        return i + 5 == StackSize ? Stack[i+4].FloatValue : 0;
    }

    private void DoRCurveLine()
    {
        var span = CurrentStackSpan;
        DoRRCurveTo(span[..^2]);
        DoRLineTo(span[^2..]);
    }

    private void DoRLineCurve()
    {
        var span = CurrentStackSpan;
        DoRLineTo(span[..^6]);
        DoRRCurveTo(span[^6..]);
    }

    #endregion

    #region CurrentPoint

    private Vector2 IncrementCurrentPointFrom(int index) => 
        IncrementCurrentPoint(Stack[index].FloatValue, Stack[index+1].FloatValue);

    private Vector2 IncrementCurrentPointFrom(in Span<DictValue> values, int index = 0) =>
        IncrementCurrentPoint(values[index].FloatValue, values[index + 1].FloatValue);

    private Vector2 IncrementCurrentPoint(float deltaX, float deltaY)
    {
        CurrentX += deltaX;
        CurrentY += deltaY;
        return Vector2.Transform(new Vector2(CurrentX, CurrentY), transform);
    }

    #endregion

    #region Flex Operations

    private void DoFlex() =>
        FlexImplementation(IncrementCurrentPoint(0,0),
            IncrementCurrentPointFrom(0),
            IncrementCurrentPointFrom(2),
            IncrementCurrentPointFrom(4),
            IncrementCurrentPointFrom(6),
            IncrementCurrentPointFrom(8),
            IncrementCurrentPointFrom(10),
            Stack[12].FloatValue);

    private void DoFlex1()
    {
        float dx = 0;
        float dy = 0;
        for (int i = 0; i < 10; i+=2)
        {
            dx += Stack[i].FloatValue;
            dy += Stack[i+1].FloatValue;
        }

        if (Math.Abs(dx) > Math.Abs(dy))
            DoHorizontalFlex1();
        else
            DoVerticalFlex1();
    }

    private void DoHorizontalFlex1()
    {
        var saved = CurrentY;
        FlexImplementation(
            IncrementCurrentPoint(0,0),
            IncrementCurrentPointFrom(0),
            IncrementCurrentPointFrom(2),
            IncrementCurrentPointFrom(4),
            IncrementCurrentPointFrom(6),
            IncrementCurrentPointFrom(8),
            IncrementCurrentPoint(Stack[10].FloatValue, saved - CurrentY),
            50);
    }
    private void DoVerticalFlex1()
    {
        var saved = CurrentX;
        FlexImplementation(
            IncrementCurrentPoint(0,0),
            IncrementCurrentPointFrom(0),
            IncrementCurrentPointFrom(2),
            IncrementCurrentPointFrom(4),
            IncrementCurrentPointFrom(6),
            IncrementCurrentPointFrom(8),
            IncrementCurrentPoint(saved - CurrentX, Stack[10].FloatValue),
            50);
    }

    private void DoHFlex() =>
        FlexImplementation(IncrementCurrentPoint(0,0),
            IncrementCurrentPoint(Stack[0].FloatValue, 0),
            IncrementCurrentPointFrom(1),
            IncrementCurrentPoint(Stack[3].FloatValue, 0),
            IncrementCurrentPoint(Stack[4].FloatValue, 0),
            IncrementCurrentPoint(Stack[5].FloatValue, -Stack[2].FloatValue),
            IncrementCurrentPoint(Stack[6].FloatValue, 0),
            50);

    private void DoHFlex1() =>
        FlexImplementation(IncrementCurrentPoint(0,0),
            IncrementCurrentPointFrom(0),
            IncrementCurrentPointFrom(2),
            IncrementCurrentPoint(Stack[4].FloatValue, 0),
            IncrementCurrentPoint(Stack[5].FloatValue, 0),
            IncrementCurrentPoint(Stack[6].FloatValue, -Stack[3].FloatValue),
            IncrementCurrentPoint(Stack[7].FloatValue, -Stack[1].FloatValue),
            50);

    private void FlexImplementation(
        Vector2 start, Vector2 control1, Vector2 control2, Vector2 flexPoint, 
        Vector2 control3, Vector2 control4, Vector2 end, float flexValueTimes100)
    {
        var dist = MinimumDistance(start, end, flexPoint);
        if (dist * 100 > flexValueTimes100)
        {
            target.CurveTo(control1, control2, flexPoint);
            target.CurveTo(control3, control4, end);
        }
        else
        {
            target.LineTo(end);
        }
    }
    
    private ValueTask<int> SendEndGlyphAsync()
    {
        if (StackSize > 0)
            target.RelativeCharWidth(Stack[0].FloatValue);
        target.EndGlyph();
        return ValueTask.FromResult(-1);
    }
    private ValueTask<int> ReturnFromSubroutineAsync() => ValueTask.FromResult(-2);

    private float MinimumDistance(Vector2 lineStart, Vector2 lineEnd, Vector2 point)
    {
        // loosly based on explanation at https://www.geeksforgeeks.org/minimum-distance-from-a-point-to-the-line-segment-using-vectors/
        // I do not check the endpoints because I want minimum distance to the line, not a line segment
        var lineVector = lineEnd - lineStart;
        var flexVector = point - lineStart;
        return Math.Abs(CrossProduct(lineVector, flexVector) / lineVector.Length());
    }

    private static float CrossProduct(Vector2 a, Vector2 b) => a.X * b.Y - a.Y * b.X;

    #endregion
}