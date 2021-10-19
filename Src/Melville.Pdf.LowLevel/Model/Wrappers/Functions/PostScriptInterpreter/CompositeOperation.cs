using System.Collections.Generic;

namespace Melville.Pdf.LowLevel.Model.Wrappers.Functions.PostScriptInterpreter
{
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
}