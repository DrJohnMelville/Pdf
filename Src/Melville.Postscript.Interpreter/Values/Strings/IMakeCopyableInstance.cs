namespace Melville.Postscript.Interpreter.Values.Strings;

internal interface IMakeCopyableInstance
{
    PostscriptValue AsCopyableValue(in MementoUnion memento);
}