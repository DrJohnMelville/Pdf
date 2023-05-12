using System.Threading.Tasks;
using Melville.INPC;
using Melville.Pdf.LowLevel.Parsing.ParserContext;

namespace Melville.Pdf.ComparingReader.Renderers
{
    public partial class OneShotPasswordSource: IPasswordSource
    {
        [FromConstructor] private readonly IPasswordSource inner;
        private bool hasBeenUsedBefore = false;
        public ValueTask<(string?, PasswordType)> GetPasswordAsync()
        {
            if (hasBeenUsedBefore) return ValueTask.FromResult(((string?)null, PasswordType.User));
            hasBeenUsedBefore = true;
            return inner.GetPasswordAsync();
        }
    }
}