using System;
using System.Buffers;
using Microsoft.Win32.SafeHandles;

namespace Melville.Pdf.LowLevel.Model.Wrappers.Functions.PostScriptInterpreter;

public interface IPostScriptOperation
{
    void Do(PostscriptStack stack);
}


public class IfOperation : IPostScriptOperation
{
    private readonly IPostScriptOperation ifBranch;
    private readonly IPostScriptOperation elseBranch;

    public IfOperation(IPostScriptOperation ifBranch) :
        this(ifBranch, PostScriptOperations.IfElse){}
    public IfOperation(IPostScriptOperation ifBranch, IPostScriptOperation elseBranch)
    {
        this.ifBranch = ifBranch;
        this.elseBranch = elseBranch;
    }

    public void Do(PostscriptStack stack) => (Math.Abs(stack.Pop()) > 0.5 ? ifBranch : elseBranch).Do(stack);
}