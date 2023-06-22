using System.Collections.Generic;

namespace Melville.Postscript.Interpreter.Values
{
    /// <summary>
    /// This interface describes Postscript Arrays arrays and strings are both arrays
    /// </summary>
    public interface IPostscriptArray : IPostscriptComposite, IEnumerable<PostscriptValue>
    {
        /// <summary>
        /// /// Get a subsequence of the given object.
        /// </summary>
        /// <param name="beginningPosition">index of the first position</param>
        /// <param name="length"></param>
        /// <returns></returns>
        public IPostscriptValueStrategy<string> IntervalFrom(int beginningPosition, int length);

        /// <summary>
        /// Overwrite 
        /// </summary>
        /// <param name="index"></param>
        /// <param name="values"></param>
        public void InsertAt(int index, IPostscriptArray values);
    }
}