using System;


namespace Melville.Pdf.LowLevel.Model.Wrappers.Functions.PostScriptInterpreter;

internal class PostscriptFunction: PdfFunction
{
    private IPostScriptOperation operation;
    public PostscriptFunction(ClosedInterval[] domain, ClosedInterval[] range, IPostScriptOperation operation) : base(domain, range)
    {
        this.operation = operation;
    }

    protected override void ComputeOverride(in ReadOnlySpan<double> input, in Span<double> result)
    {
        var stack = StackWithInputs(input);
        operation.Do(stack);
        CopyStackToOutputs(result, stack);
    }

    private static PostscriptStack StackWithInputs(ReadOnlySpan<double> input)
    {
        var stack = new PostscriptStack();
        foreach (var inp in input)
        {
            stack.Push(inp);
        }

        return stack;
    }

    private static void CopyStackToOutputs(Span<double> result, PostscriptStack stack)
    {
        stack.AsSpan()[..result.Length].CopyTo(result);
    }
}