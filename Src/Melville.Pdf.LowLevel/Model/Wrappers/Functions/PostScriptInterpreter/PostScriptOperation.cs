using System;
using System.Buffers;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Melville.Pdf.LowLevel.Model.Primitives;

namespace Melville.Pdf.LowLevel.Model.Wrappers.Functions.PostScriptInterpreter
{
    public class PostscriptStack : List<double>
    {
        public void Push(double item) => Add(item);

        public double Pop()
        {
            var last = this.Count - 1;
            var ret = this[last];
            RemoveAt(last);
            return ret;
        }

        public double Peek() => this[Count - 1];

        public Span<double> AsSpan() => CollectionsMarshal.AsSpan(this);

        public void Exchange()
        {
            var span = AsSpan();
            (span[^2], span[^1]) = (span[^1], span[^2]);
        }

        public double Peek(int item) => AsSpan()[^(item + 1)];
    }
    public interface IPostScriptOperation
    {
        void Do(PostscriptStack stack);
    }

    public static partial class PostScriptOperations
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
    }

    public class PostScriptSpecialOperations: IPostScriptOperation
    {
        public static readonly IPostScriptOperation OpenBrace = new PostScriptSpecialOperations();
        public static readonly IPostScriptOperation CloseBrace = new PostScriptSpecialOperations();
        public static readonly IPostScriptOperation OutOfChars = new PostScriptSpecialOperations();
        
        private PostScriptSpecialOperations()
        {
        }

        public void Do(PostscriptStack stack)
        {
            throw new System.NotSupportedException();
        }
    }
    
    public class CompositeOperation: IPostScriptOperation
    {
        private List<IPostScriptOperation> operations = new();
        public void AddOperation(IPostScriptOperation op) => operations.Add(op);
        public void Do(PostscriptStack stack)
        {
            foreach (var op in operations)
            {
                op.Do(stack);
            }
        }
    }

    public class PushConstantOperation: IPostScriptOperation
    {
        private double value;

        public PushConstantOperation(double value)
        {
            this.value = value;
        }

        public void Do(PostscriptStack stack)
        {
            stack.Push(value);
        }
    }
}