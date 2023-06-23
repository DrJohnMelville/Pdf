using System;
using System.Collections.Generic;
using System.Linq;
using Melville.INPC;
using Melville.Postscript.Interpreter.InterpreterState;
using Melville.Postscript.Interpreter.Values;
using Melville.Postscript.Interpreter.Values.Composites;
// used in generated files
using Melville.Postscript.Interpreter.Values.Execution;
using Melville.Postscript.Interpreter.Values.Strings;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.VisualBasic.CompilerServices; // used in generated files
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Melville.Postscript.Interpreter.FunctionLibrary;

/// <summary>
/// This is a library of operators for the postscript interpreter
/// </summary>
[MacroCode("""
        /// <summary>
        /// ~2~ 
        /// </summary>
        public static IExternalFunction ~0~ = new ~0~BuiltInFuncImpl();
        private sealed class ~0~BuiltInFuncImpl: BuiltInFunction
        {
            public override void Execute(PostscriptEngine engine, in PostscriptValue value)
            {
                ~1~
            }
        }

        """)]
// Stack operators
[MacroItem("Pop", "engine.OperandStack.Pop();", "Discard the top element on the stack.")]
[MacroItem("Exchange", "engine.OperandStack.Exchange();", "Switch the top two items on the stack.")]
[MacroItem("Duplicate", "engine.OperandStack.Duplicate();", "Duplicate the top item on the stack.")]
[MacroItem("CopyTop", "engine.OperandStack.PolymorphicCopy();", "Copy the top item on the stack or copy a composite object.")]
[MacroItem("IndexOperation", "engine.OperandStack.IndexOperation(engine.PopAs<int>());", "Retrieve the item n items down the stack.")]
[MacroItem("Roll", "engine.OperandStack.Roll(engine.PopAs<int>(), engine.PopAs<int>());", "Roll the stack.")]
[MacroItem("ClearStack", "engine.OperandStack.Clear();", "Clear the stack.")]
[MacroItem("CountStack", "engine.OperandStack.PushCount();", "Count the items on the stack.")]
[MacroItem("ClearToMark", "engine.OperandStack.ClearToMark();", "Clear down to and including a mark")]
[MacroItem("CountToMark", "engine.Push(engine.OperandStack.CountToMark());", "Count items above the topmost mark mark")]

// Math Operators
[MacroItem("RealDivide", "engine.PopAs(out double a, out double b); engine.Push(a/b);", "Floating point division")]
[MacroItem("IntegerDivide", "engine.PopAs(out long a, out long b); engine.Push(a/b);", "Integer division")]
[MacroItem("SquareRoot", "engine.Push(Math.Sqrt(engine.PopAs<double>()));","Square Root")]
[MacroItem("ArcTangent", "engine.PopAs(out double num, out double den);engine.Push(Math.Atan2(num,den) * 180.0 / Math.PI);", "Arctangent")]
[MacroItem("Sine", "engine.Push(Math.Sin(engine.PopAs<double>() * Math.PI/180.0));", "Sine")]
[MacroItem("Cosine", "engine.Push(Math.Cos(engine.PopAs<double>() * Math.PI/180.0));", "Cosine")]
[MacroItem("RaiseToPower", "engine.PopAs(out double num, out double exp);engine.Push(Math.Pow(num,exp));", "Raise number to a power")]
[MacroItem("Log10", "engine.Push(Math.Log10(engine.PopAs<double>()));", "Log base 10")]
[MacroItem("NaturalLog", "engine.Push(Math.Log(engine.PopAs<double>()));", "Log base e")]
[MacroItem("SetRandomSeed", "engine.Random.State = (uint)engine.PopAs<long>();", "Set random seed")]
[MacroItem("GetRandomSeed", "engine.Push(engine.Random.State);", "get random seed")]
[MacroItem("GetNextRandom", "engine.Push(engine.Random.Next());", "get next random number")]

//Array Operators
[MacroItem("EmptyArray", "engine.Push(PostscriptValueFactory.CreateSizedArray(engine.PopAs<int>()));", "Create an array of a given number of nulls")]
[MacroItem("ArrayFromStack", "engine.OperandStack.MarkedSpanToArray(false);", "Create an array from the topmost marked region on the stack")]
[MacroItem("CompositeLength", "engine.Push(engine.PopAs<IPostscriptComposite>().Length);", "Create an array from the topmost marked region on the stack")]
[MacroItem("CompositeGet", """
    var index = engine.OperandStack.Pop();
    var comp = engine.PopAs<IPostscriptComposite>();
    engine.Push(comp.Get(index));
    """, "Retrieve a value from a composite.")]
[MacroItem("CompositePut", """
    var item = engine.OperandStack.Pop();
    var index = engine.OperandStack.Pop();
    var comp = engine.PopAs<IPostscriptComposite>();
    comp.Put(index, item);
    """, "Store a value in a composite.")]
[MacroItem("GetInterval", """
        engine.PopAs<int, int>(out var index, out var length);
        var token = engine.OperandStack.Pop();
        var array = token.Get<IPostscriptArray>();
        engine.Push(
            new PostscriptValue(array.IntervalFrom(index, length), token.ExecutionStrategy, 0));
    """, "Extract a subarray from an array.")]
[MacroItem("PutInterval", """
        var source = engine.PopAs<IPostscriptArray>();
        engine.PopAs<IPostscriptArray, int>(out var target, out var index);
        target.InsertAt(index, source);
    """, "Write an array into another array.")]
[MacroItem("AStore", """
        engine.Push(engine.PopAs<PostscriptArray>().PushAllFrom(engine.OperandStack));
        """, "Store the operand stack into an array.")]
[MacroItem("ALoad", """
        var token = engine.OperandStack.Pop();
        var array = token.Get<PostscriptArray>();
        array.PushAllTo(engine.OperandStack);
        engine.Push(token);
        """, "Unpack an array into the operand stack.")]

// "Packed array" operators -- We implement packed arrays as normal arrays
[MacroItem("CurrentPacking", "engine.Push(engine.PackingMode);","Read current packing mode")]
[MacroItem("SetPacking", "engine.PackingMode = engine.PopAs<bool>();","Read current packing mode")]
[MacroItem("PackedArray", "engine.OperandStack.CreatePackedArray();","Create an array from the stack")]

// Conversion operators
[MacroItem("MakeExecutable", """
        engine.Push(engine.OperandStack.Pop().AsExecutable());
    """, "Make the top token executable")]
[MacroItem("MakeLitreral", """
        engine.Push(engine.OperandStack.Pop().AsLiteral());
    """, "Make the top token executable")]
[MacroItem("IsExecutable", """
        engine.Push(engine.OperandStack.Pop().ExecutionStrategy.IsExecutable);
        """, "Check if top token is executable")]
[MacroItem("ProcFromStack", "engine.OperandStack.MarkedSpanToArray(true);", "Create an array from the topmost marked region on the stack")]
[MacroItem("Nop", "// do nothing", "No Operation -- does nothing")]
[MacroItem("FakeAccessCheck", "engine.OperandStack.Pop(); engine.Push(true);", "No Operation -- does nothing")]
[MacroItem("ConvertToInt", "engine.Push(engine.PopAs<long>());", "Convert To int")]
[MacroItem("ConvertToDouble", "engine.Push(engine.PopAs<double>());", "Convert To int")]
[MacroItem("ConvertToName", """
        var op = engine.OperandStack.Pop();
        var text = op.Get<Memory<byte>>();
        engine.Push(PostscriptValueFactory.CreateString(text.Span, 
            op.ExecutionStrategy.IsExecutable ? 
                StringKind.Name : StringKind.LiteralName));
    """, "Convert To int")]
[MacroItem("ConvertToString", """
        engine.Push(
            PostscriptValueFactory.CreateString(
                engine.OperandStack.Pop().ToString(), StringKind.String));
    """, "Convert To int")]
[MacroItem("ConvertToRadixString", """
        var buffer = engine.OperandStack.Pop();
        var radix = engine.PopAs<int>();
        var number = engine.OperandStack.Pop();
        engine.Push(
            new RadixPrinter(number, radix, buffer.Get<Memory<byte>>()).CreateValue());
    """, "Convert number to string with a particular radix")]

// Control Operators
[MacroItem("Execute", """
        var token = engine.OperandStack.Pop();
        token.ExecutionStrategy.Execute(engine, token);
    """, "Execute the top token on the stack")]
[MacroItem("If", """
        var proc = engine.OperandStack.Pop();
        if (engine.PopAs<bool>()) proc.ExecutionStrategy.Execute(engine, proc);
    """, "If operatiom")]
[MacroItem("IfElse", """
        var elseProc = engine.OperandStack.Pop();
        var proc = engine.OperandStack.Pop();
        if (engine.PopAs<bool>()) 
            proc.ExecutionStrategy.Execute(engine, proc);
        else
            elseProc.ExecutionStrategy.Execute(engine, elseProc);
    """, "If/Else operation")]
[MacroItem("For", """
        var proc = engine.OperandStack.Pop();
        var limit = engine.PopAs<double>();
        engine.PopAs<double, double>(out var initial, out var increment);
        engine.ExecutionStack.PushLoop(
            LoopSources.For(initial, increment, limit, proc), "For Loop"u8);
    """, "For Loop")]
[MacroItem("Repeat", """
        var proc = engine.OperandStack.Pop();
        var count = engine.PopAs<int>();
        engine.ExecutionStack.PushLoop(
            LoopSources.Repeat(count, proc), "Repeat Loop"u8);
    """, "Repeat")]
[MacroItem("Loop", """
        var proc = engine.OperandStack.Pop();
        engine.ExecutionStack.PushLoop(LoopSources.Loop(proc), "Loop Loop"u8);
    """, "Loop")]
[MacroItem("ForAll", """
        var proc = engine.OperandStack.Pop();
        var compsite = engine.PopAs<IPostscriptComposite>();
        engine.ExecutionStack.PushLoop(LoopSources.ForAll(compsite, proc), "ForAll Loop");
    """, "ForAll Loop")]
[MacroItem("Exit", "engine.ExecutionStack.ExitLoop();", "exit out of an enclosing loop")]
[MacroItem("StopRegion", """
        engine.ExecutionStack.Push(
            new(new StopContext(engine.OperandStack.Pop())), "Stop Context"u8);
    """, "Run a proc in a stop context")]
[MacroItem("Stop", "engine.ExecutionStack.HandleStop();", "Jump out of a stop region")]
[MacroItem("CountExecutionStack", "engine.Push((long)engine.ExecutionStack.Count);", "Count exection stack")]
[MacroItem("ExecStack", """
        var array = engine.PopAs<PostscriptArray>();
        int len = engine.ExecutionStack.CopyTo(array);
        engine.Push(array.InitialSubArray(len, PostscriptBuiltInOperations.PushArgument));
    """, "copy execution stack to an array")]
[MacroItem("Quit", "engine.ExecutionStack.Clear();","Stop interpreting")]
[MacroItem("Start", "//Do nothing -- it is a no-op", "Explicitly executing start is undefined.  We implement it as a  no-op for maximum compatibility")]

// relational and bitwiseOperators
[MacroItem("OpEqual", """
        engine.Push(EqualOperatorImpl.IsEqual(engine));
    """, "Implement the equals operation")]
[MacroItem("OpNotEqual", """
        engine.Push(!EqualOperatorImpl.IsEqual(engine));
    """, "Implement the equals operation")]
[MacroItem("Not", """
        var op = engine.OperandStack.Pop();
        engine.Push(op.TryGet(out long val)?PostscriptValueFactory.Create(~val):
            PostscriptValueFactory.Create(!op.Get<bool>()));
    """, "Implement the Not operation")]
[MacroItem("GreaterThanOrEqual", """
        engine.Push(EqualOperatorImpl.Compare(engine) >= 0);
    """, "Implement the &gt;= operation")]
[MacroItem("GreaterThan", """
        engine.Push(EqualOperatorImpl.Compare(engine) > 0);
    """, "Implement the &gt; operation")]
[MacroItem("LessThanOrEqual", """
        engine.Push(EqualOperatorImpl.Compare(engine) <= 0);
    """, "Implement the &lt;= operation")]
[MacroItem("LessThan", """
        engine.Push(EqualOperatorImpl.Compare(engine) < 0);
    """, "Implement the &lt; operation")]
[MacroItem("BitShift", """
        engine.PopAs(out long baseVal, out int shift);
        engine.Push(shift >= 0 ? baseVal << shift: baseVal >> -shift);
    """, "Implement the &lt; operation")]

[MacroItem("CreateDictionary", 
    "engine.Push(PostscriptValueFactory.CreateSizedDictionary(engine.PopAs<int>()));", 
    "Create a dictionary with a give size")]
[MacroItem("DictCapacity",
    "engine.Push(engine.PopAs<PostscriptDictionary>().MaxLength);", 
    "Retrieve the capacity of a dictionary")]
[MacroItem("DictionaryFromStack",
    "engine.OperandStack.MarkedSpanToDictionary();", 
    "Create a dictionary from the marked stack")]
[MacroItem("DefineInTopDict", """
        var (key, valueToSet) = engine.PopTwo();
        engine.DictionaryStack.Peek().Put(key, valueToSet);
    """, 
    "Define a key in the topmost dictionary on the dictionary stack")]
[MacroItem("LookupInDictStack", """
    engine.Push(engine.DictionaryStack.Get(engine.OperandStack.Pop()));
    """, 
    "Define a key in the topmost dictionary on the dictionary stack")]
[MacroItem("PushDictionaryStack", """
    engine.DictionaryStack.Push(engine.OperandStack.Pop());
    """, 
    "Push a dictionary onto the dictionary stack`")]
[MacroItem("PopDictionaryStack", """
    engine.DictionaryStack.Pop();
    """, 
    "Push a dictionary onto the dictionary stack`")]
[MacroItem("DictionaryStore", """
        var (key, val) = engine.PopTwo();
        engine.DictionaryStack.Store(key, val);
    """, 
    "Push a dictionary onto the dictionary stack`")]
[MacroItem("DictionaryKnown", """
        var (dict, key) = engine.PopTwo();
        engine.Push(dict.Get<PostscriptDictionary>().TryGet(key,out _));
    """, 
    "True if the dictionary contains the key, false otherwise`")]
[MacroItem("Undefine", """
        var (dict, key) = engine.PopTwo();
        dict.Get<PostscriptDictionary>().Undefine(key);
    """, 
    "Remove key from the dictionary if it exists")]
[MacroItem("Where", """
    engine.DictionaryStack.PostscriptWhere(engine.OperandStack);   
    """, 
    "Find the dictionary that contains a given key.")]
[MacroItem("CurrentDictionary", """
    engine.Push(engine.DictionaryStack.CurrentDictAsValue);
    """, 
    "Push the currently selected dictionary")]
[MacroItem("CountDictStack", """
    engine.Push(engine.DictionaryStack.Count);
    """, 
    "Push the size of the dictionary stack")]
[MacroItem("DictStack", """
        engine.Push(
            engine.DictionaryStack.WriteStackTo(engine.PopAs<PostscriptArray>()));
     """, 
    "Copy dictstack to the an array.")]
[MacroItem("ClearDictStack", """
     engine.DictionaryStack.ResetToBottom3();
     """, 
    "Copy dictstack to the an array.")]

// string search operators
[MacroItem("AnchorSearch", """
        var seek = engine.OperandStack.Pop();
        engine.OperandStack.Peek().Get<PostscriptLongString>().DoAnchorSearch(
            engine.OperandStack, seek);
     """,
    "Search for a string at the beginning of a string.")]
[MacroItem("Search", """
        var seek = engine.OperandStack.Pop();
        engine.OperandStack.Peek().Get<PostscriptLongString>().DoSearch(
            engine.OperandStack, seek);
     """,
    "Search for a string at the beginning of a string.")]
[MacroItem("Token", """
         engine.PopAs<ITokenSource>().GetToken(engine.OperandStack);
     """,
    "Search for a string at the beginning of a string.")]
public static partial class PostscriptOperators
{
#if DEBUG
    private static void XX(PostscriptEngine engine)
    {
    } 
#endif

    [MacroCode("""
        /// <summary>
        /// ~2~ 
        /// </summary>
        public static IExternalFunction ~0~ = new ~0~BuiltInFuncImpl();
        private sealed class ~0~BuiltInFuncImpl: IntOrBinaryOperation<double>
        {
            protected override PostscriptValue Op(long a, long b) => ~1~;
            protected override PostscriptValue Op(double a, double b) => ~1~;
        }

        """)]
    [MacroItem("Add", "a + b", "Add two numbers.")]
    [MacroItem("Modulo", "a % b", "Remainder after integer division.")]
    [MacroItem("Multiply", "a * b", "Multiply two numbers.")]
    [MacroItem("Subtract", "a - b", "difference of two numbers.")]
    static partial void NumberOpsHolder();

    [MacroCode("""
        /// <summary>
        /// ~2~ 
        /// </summary>
        public static IExternalFunction ~0~ = new ~0~BuiltInFuncImpl();
        private sealed class ~0~BuiltInFuncImpl: IntOrBinaryOperation<bool>
        {
            protected override PostscriptValue Op(long a, long b) => ~1~;
            protected override PostscriptValue Op(bool a, bool b) => ~1~;
            protected override bool IsIntegerType(in PostscriptValue value) =>
                value.IsNumber;
        }

        """)]
    [MacroItem("And", "a & b", "Add two numbers or booleans.")]
    [MacroItem("Or", "a | b", "Or two numbers or booleans.")]
    [MacroItem("Xor", "a ^ b", "Xor two numbers or booleans.")]
    static partial void BitOpsHolder();

    [MacroCode("""
        /// <summary>
        /// ~2~ 
        /// </summary>
        public static IExternalFunction ~0~ = new ~0~BuiltInFuncImpl();
        private sealed class ~0~BuiltInFuncImpl: IntOrUnaryOperation<double>
        {
            protected override PostscriptValue Op(long a) => ~1~;
            protected override PostscriptValue Op(double a) => ~1~;
        }

        """)]
    [MacroItem("AbsoluteValue", "Math.Abs(a)", "AbsoluteValue")]
    [MacroItem("Negative", "a * -1", "Additive inverse.")]
    static partial void NumberUnaryOpsHolder();

    [MacroCode("""
        /// <summary>
        /// ~2~ 
        /// </summary>
        public static IExternalFunction ~0~ = new ~0~BuiltInFuncImpl();
        private sealed class ~0~BuiltInFuncImpl: IntOrUnaryOperation<double>
        {
            protected override PostscriptValue Op(long a) => a;
            protected override PostscriptValue Op(double a) => ~1~;
        }

        """)]
    [MacroItem("Ceiling", "Math.Ceiling(a)", "Smallest whole number bigger than item.")]
    [MacroItem("Floor", "Math.Floor(a)", "Largest whole number less than argument.")]
    [MacroItem("Round", "Math.Round(a, MidpointRounding.AwayFromZero)", "Round argument to nearest whole integer")]
    [MacroItem("Truncate", "Math.Truncate(a)", "Truncate fractional part of a number")]
    static partial void DoubleOnlyOpsHolder();

}