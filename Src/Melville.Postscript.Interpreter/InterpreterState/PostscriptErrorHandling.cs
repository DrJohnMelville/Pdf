using System;
using System.Collections.Generic;
using Melville.INPC;
using Melville.Postscript.Interpreter.Values;
using Melville.Postscript.Interpreter.Values.Interfaces;

namespace Melville.Postscript.Interpreter.InterpreterState;

public readonly partial struct PostscriptErrorHandling
{
    [FromConstructor] private readonly PostscriptEngine engine;
    [FromConstructor] private readonly Exception exception;
    [FromConstructor] private readonly PostscriptValue offendingItem;

    public PostscriptValue Handle() => exception switch
    {
        PostscriptNamedErrorException e => Handle(e.ErrorName),
        KeyNotFoundException => Handle("undefined"),
        _=> PostscriptValueFactory.CreateNull()
    };

    private PostscriptValue Handle(string postscriptErrorName)
    {
        engine.OperandStack.Push(offendingItem);
        return engine.SystemDict.TryGetAs<IPostscriptComposite>("errordict", out var errorDict) &&
               errorDict.TryGet(postscriptErrorName, out var proc)
            ? proc
            : PostscriptValueFactory.Create(new DefaultErrorProc(postscriptErrorName));
    }
}