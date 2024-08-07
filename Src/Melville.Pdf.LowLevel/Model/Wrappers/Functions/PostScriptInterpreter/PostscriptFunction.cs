﻿using System;
using System.Collections.Generic;
using Melville.Postscript.Interpreter.FunctionLibrary;
using Melville.Postscript.Interpreter.InterpreterState;
using Melville.Postscript.Interpreter.Tokenizers;
using Melville.Postscript.Interpreter.Values;
using Melville.Postscript.Interpreter.Values.Composites;


namespace Melville.Pdf.LowLevel.Model.Wrappers.Functions.PostScriptInterpreter;

/// <summary>
/// This is a factory class for the postscript interpreter used to interpret type 4 PDF
/// functions implemented in postscript.
/// </summary>
public static class SharedPostscriptParser
{
    private static readonly IPostscriptDictionary operations =
        PostscriptOperatorCollections.BaseLanguage();

    /// <summary>
    /// This is the basic postscript engine used to interpret postscript functions.
    /// </summary>
    /// <returns>A new postscript engine</returns>
    public static PostscriptEngine BasicPostscriptEngine() => new PostscriptEngine(operations).WithImmutableStrings();
}

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
        var engine = SharedPostscriptParser.BasicPostscriptEngine();
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