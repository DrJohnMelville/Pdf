using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Melville.Pdf.LowLevel.Writers.ObjectWriters;
using Melville.Postscript.Interpreter.FunctionLibrary;
using Melville.Postscript.Interpreter.InterpreterState;
using Melville.Postscript.Interpreter.Tokenizers;
using Melville.Postscript.Interpreter.Values;


namespace Melville.Pdf.LowLevel.Model.Wrappers.Functions.PostScriptInterpreter;

internal class PostscriptFunction: PdfFunction
{
    private readonly PretokenizedSource operation;
    public PostscriptFunction(
        ClosedInterval[] domain, ClosedInterval[] range, 
        IEnumerable<PostscriptValue> operation) : base(domain, range)
    {
        this.operation = new PretokenizedSource(operation);
    }

    protected override void ComputeOverride(
        in ReadOnlySpan<double> input, in Span<double> result)
    {
        var engine = new PostscriptEngine().WithBaseLanguage();
        PushInputs(input, engine);
        engine.Execute(operation);
        PopOutputs(result, engine);
    }

    private static void PushInputs(ReadOnlySpan<double> input, PostscriptEngine engine)
    {
        foreach (var inputItem in input)
        {
            engine.OperandStack.Push(inputItem);
        }
    }

    private static void PopOutputs(Span<double> result, PostscriptEngine engine)
    {
        for (int i = result.Length - 1; i >= 0; i--)
            result[i] = ExtractValue(engine.OperandStack.Pop());
    }

    private static double ExtractValue(PostscriptValue value)
    {
        if (value.TryGet(out double doubleValue)) return doubleValue;
        return value.Get<bool>() ? -1.0 : 0.0;
    }
}