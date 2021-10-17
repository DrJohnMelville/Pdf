using System.Collections.Generic;

namespace Melville.Pdf.LowLevel.Model.Wrappers.Functions.PostScriptInterpreter
{
    public interface IPostScriptOperation
    {
        void Do(Stack<double> stack);
    }

    public class PostScriptSpecialOperations: IPostScriptOperation
    {
        public static readonly IPostScriptOperation OpenBrace = new PostScriptSpecialOperations();
        public static readonly IPostScriptOperation CloseBrace = new PostScriptSpecialOperations();
        public static readonly IPostScriptOperation OutOfChars = new PostScriptSpecialOperations();
        
        private PostScriptSpecialOperations()
        {
        }

        public void Do(Stack<double> stack)
        {
            throw new System.NotSupportedException();
        }
    }
    
    public class CompositeOperation: IPostScriptOperation
    {
        private List<IPostScriptOperation> operations = new();
        public void AddOperation(IPostScriptOperation op) => operations.Add(op);
        public void Do(Stack<double> stack)
        {
            foreach (var op in operations)
            {
                op.Do(stack);
            }
        }
    }
    public class MulOp: IPostScriptOperation
    {
        public void Do(Stack<double> stack)
        {
            stack.Push(stack.Pop()*stack.Pop());
        }
    }

    public class PushConstantOperation: IPostScriptOperation
    {
        private double value;

        public PushConstantOperation(double value)
        {
            this.value = value;
        }

        public void Do(Stack<double> stack)
        {
            stack.Push(value);
        }
    }
}