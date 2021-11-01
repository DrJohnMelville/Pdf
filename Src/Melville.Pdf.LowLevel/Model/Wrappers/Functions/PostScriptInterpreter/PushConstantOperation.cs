namespace Melville.Pdf.LowLevel.Model.Wrappers.Functions.PostScriptInterpreter;

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