using Melville.Postscript.Interpreter.Values;
using Melville.Postscript.Interpreter.Values.Composites;

namespace Melville.Postscript.Interpreter.InterpreterState
{
    internal readonly struct EngineStackState
    {
        private readonly StackMarker<PostscriptValue> values;
        private readonly StackMarker<IPostscriptDictionary> dictionaries;
        private readonly StackMarker<ExecutionContext> execution;

        public EngineStackState(
            PostscriptStack<PostscriptValue> values, 
            PostscriptStack<IPostscriptDictionary> dictionaries, 
            PostscriptStack<ExecutionContext> execution)
        {
            this.values = new(values);
            this.dictionaries = new(dictionaries);
            this.execution = new(execution);
        }

        public void Rollback()
        {
            values.Rollback();
            dictionaries.Rollback();
            execution.Rollback();
        }

        public void Commit()
        {
            values.Commit();
            dictionaries.Commit();
            execution.Commit();
        }
    }
}