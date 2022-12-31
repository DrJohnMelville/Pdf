namespace Melville.Pdf.LowLevel.Model.Wrappers.Functions.PostScriptInterpreter;

internal interface IPostScriptOperation
{
    void Do(PostscriptStack stack);
}