using System.IO;
using System.Threading.Tasks;

namespace Melville.Pdf.LowLevel.Parsing.ParserContext
{
    public enum PasswordType
    {
        User,
        Owner
    }
    public interface IPasswordSource
    {
        ValueTask<(string?, PasswordType)> GetPassword();
    }

    public class NullPasswordSource : IPasswordSource
    {
        public ValueTask<(string?, PasswordType)> GetPassword() =>
            throw new InvalidDataException(
                "This file is encrypted.  To open encrypted files pass an IPasswordSource to the " +
                "ParsingFileOwner constructor.");
    }

    public class ConstantPasswordSource : IPasswordSource
    {
        private readonly PasswordType type;
        private readonly string?[] passwords;
        private int next;

        public ConstantPasswordSource(PasswordType type, params string?[] passwords)
        {
            this.type = type;
            this.passwords = passwords;
        }

        public ValueTask<(string?, PasswordType)> GetPassword() => new((TryGetNextPassword(), type));
        private string? TryGetNextPassword() => HasNextPassword() ? NextPassword() : null;
        private string? NextPassword() => passwords[next++];
        private bool HasNextPassword() => next < passwords.Length;
    }
}