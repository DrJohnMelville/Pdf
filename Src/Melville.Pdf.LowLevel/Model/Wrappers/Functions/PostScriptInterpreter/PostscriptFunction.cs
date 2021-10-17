using System;
using System.Collections.Generic;


namespace Melville.Pdf.LowLevel.Model.Wrappers.Functions.PostScriptInterpreter
{
    public class PostscriptFunction: PdfFunction
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

        private static Stack<double> StackWithInputs(ReadOnlySpan<double> input)
        {
            var stack = new Stack<double>();
            foreach (var inp in input)
            {
                stack.Push(inp);
            }

            return stack;
        }

        private static void CopyStackToOutputs(Span<double> result, Stack<double> stack)
        {
            for (int i = result.Length - 1; i >= 0; i--)
            {
                result[i] = stack.Pop();
            }
        }
    }
}