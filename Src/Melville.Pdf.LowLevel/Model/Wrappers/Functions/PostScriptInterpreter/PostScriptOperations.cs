using System;
using System.Collections.Generic; // used by the generated 
using Melville.INPC;
using Melville.Pdf.LowLevel.Model.Primitives;

namespace Melville.Pdf.LowLevel.Model.Wrappers.Functions.PostScriptInterpreter;

           // sentinels
[MacroItem("Open_Brace", "throw new NotSupportedException();")]
[MacroItem("Close_Brace", "throw new NotSupportedException();")]
[MacroItem("If", "throw new NotSupportedException();")]
[MacroItem("IfElse", "")]
            // arithmetic operators
[MacroItem("Abs", "s.Push(Math.Abs(s.Pop()));")]
[MacroItem("Add", "s.Push(s.Pop()+s.Pop());")]
[MacroItem("Atan", "var den = s.Pop();var num = s.Pop(); s.Push(CanonicalDegrees(RadToDeg( Math.Atan2(num, den))));")]
[MacroItem("Ceiling", "s.Push(Math.Ceiling(s.Pop()));")]
[MacroItem("Cos", "s.Push(Math.Cos(DegToRad(s.Pop())));")]
[MacroItem("Cvi", "s.Push((int)s.Pop());")]
[MacroItem("Cvr", "")]
[MacroItem("Div", "var den = s.Pop();var num = s.Pop(); s.Push(num/den);")]
[MacroItem("Exp", "var exp = s.Pop();var @base = s.Pop(); s.Push(Math.Pow(@base, exp));")]
[MacroItem("Floor", "s.Push(Math.Floor(s.Pop()));")]
[MacroItem("Idiv", "var den = (long)s.Pop();var num = (long)s.Pop(); s.Push(num/den);")]
[MacroItem("ln", "s.Push(Math.Log(s.Pop()));")]
[MacroItem("log", "s.Push(Math.Log10(s.Pop()));")]
[MacroItem("Mod", "var den = (long)s.Pop();var num = (long)s.Pop(); s.Push(num%den);")]
[MacroItem("Mul", "s.Push(s.Pop()*s.Pop());")]
[MacroItem("Neg", "s.Push(-s.Pop());")]
[MacroItem("Round", "s.Push(PostscriptRound(s.Pop()));")]
[MacroItem("Sin", "s.Push(Math.Sin(DegToRad(s.Pop())));")]
[MacroItem("Sqrt", "s.Push(Math.Sqrt(s.Pop()));")]
[MacroItem("Sub", "var den = s.Pop();var num = s.Pop(); s.Push(num - den);")]
[MacroItem("Truncate", "s.Push(Math.Truncate(s.Pop()));")]
            
            // relational boolean and butwise operators
[MacroItem("And", "var b = (long) s.Pop(); var a = (long)s.Pop(); s.Push(a&b);")]
[MacroItem("Or", "var b = (long) s.Pop(); var a = (long)s.Pop(); s.Push(a|b);")]
[MacroItem("Xor", "var b = (long) s.Pop(); var a = (long)s.Pop(); s.Push(a^b);")]
[MacroItem("Not", "s.Push(~(long)s.Pop());")]
[MacroItem("BitShift", "var b = (int) s.Pop(); var a = (long)s.Pop(); s.Push(PostscriptBitShift(a,b));")]
[MacroItem("Eq", "s.Push(PostscriptEqual(s.Pop(),s.Pop()));")]
[MacroItem("Ne", "s.Push(PostscriptNotEqual(s.Pop(),s.Pop()));")]
[MacroItem("True", "s.Push(-1.0);")]
[MacroItem("False", "s.Push(0.0);")]
[MacroItem("Ge", "var b = s.Pop(); var a = s.Pop(); s.Push(a>=b?-1:0);")]
[MacroItem("Gt", "var b = s.Pop(); var a = s.Pop(); s.Push(a> b?-1:0);")]
[MacroItem("Le", "var b = s.Pop(); var a = s.Pop(); s.Push(a<=b?-1:0);")]
[MacroItem("Lt", "var b = s.Pop(); var a = s.Pop(); s.Push(a< b?-1:0);")]
            
            // stack operators
[MacroItem("Copy", "PostscriptCopy(s);")]
[MacroItem("Dup", "s.Push(s.Peek());")]
[MacroItem("Exch", "s.Exchange();")]
[MacroItem("Index", "s.Push(s.Peek((int)s.Pop()));")]
[MacroItem("Pop", "s.Pop();")]
[MacroItem("Roll", "var delta= (int) s.Pop(); var count = (int)s.Pop(); RollSpan(s.AsSpan()[^count..], delta);")]
[MacroCode("""
    internal sealed class Operation~0~:IPostScriptOperation
    {
        public void Do(PostscriptStack s) 
        {
            ~1~
        }
    }
    """)]
[MacroCode("internal static readonly IPostScriptOperation ~0~ = new Operation~0~();")]
[MacroCode("""    dict.Add(FnvHash.HashLowerCase("~0~"u8), PostScriptOperations.~0~);""", 
    Prefix = """
             internal static Dictionary<uint, IPostScriptOperation> CreateDictionary() 
                 {
                     var dict = new Dictionary<uint, IPostScriptOperation>(); 
             """,
    Postfix = "return dict; }")]
internal static partial class PostScriptOperations
{
    private static double DegToRad(double angle) => angle * Math.PI / 180;
    private static double RadToDeg(double angle) => angle * 180 / Math.PI;
    private static double CanonicalDegrees(double angle) => (angle + 360.0) % 360.0;
    private static double PostscriptRound(double d)
    {
        var floor = Math.Round(d, MidpointRounding.ToNegativeInfinity);
        return d - floor >= 0.5 ? floor + 1 : floor;
    }

    private static long PostscriptBitShift(long val, int shift) => 
        shift >= 0 ? val << shift : val >> -shift;

    private static double PostscriptEqual(double a, double b) =>
        (Math.Abs(a - b) < 0.0001) ? -1.0 : 0.0;
    private static double PostscriptNotEqual(double a, double b) =>
        (Math.Abs(a - b) >= 0.0001) ? -1.0 : 0.0;

    private static void PostscriptCopy(PostscriptStack s)
    {
        int count = (int)s.Pop();
        if (count < 0) throw new PdfParseException("Cannot copy a negative amount");
        Span<double> buffer = s.AsSpan()[^count..];
        PushSpan(s, buffer);
    }
    private static void PushSpan(PostscriptStack s, in Span<double> buffer)
    {
        foreach (var item in buffer)
        {
            s.Push(item);
        }
    }

    private static void RollSpan(Span<double> span, int delta)
    {
        int initialSpot = delta > 0 ? 
            delta % span.Length : 
            span.Length - ((-delta) % span.Length) % span.Length;
        if (initialSpot == 0) return;
        Span<double> buffer = stackalloc double[span.Length];
        for (int i = 0; i < span.Length; i++)
        {
            buffer[(i + initialSpot) % span.Length] = span[i];
        }
        buffer.CopyTo(span);
    }
}