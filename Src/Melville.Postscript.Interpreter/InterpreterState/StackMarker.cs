namespace Melville.Postscript.Interpreter.InterpreterState
{
    internal readonly struct StackMarker<T>
    {
        private readonly PostscriptStack<T> stack;
        private readonly int initialSize;

        public StackMarker(PostscriptStack<T> stack)
        {
            this.stack = stack;
            initialSize = stack.Count;
        }

        public void Commit() => stack.ClearAfterPop(initialSize);

        public void Rollback() => stack.RollbackTo(initialSize);
    }
}