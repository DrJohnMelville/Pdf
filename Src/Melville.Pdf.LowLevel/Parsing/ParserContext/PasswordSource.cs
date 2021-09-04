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
}