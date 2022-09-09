using System.IO;
using System.Threading.Tasks;
using Melville.INPC;

namespace Melville.Pdf.LowLevel.Parsing.ParserContext;

public enum PasswordType
{
    User,
    Owner
}

/// <summary>
/// This interface is implemented by a PDF consumer to provide passwords to the library when attempting to open
/// a password protected PDF.
/// </summary>
public interface IPasswordSource
{
    /// <summary>
    /// Return a password to attempt to decode a password encrypted pdf.
    /// </summary>
    /// <returns>
    /// A tuple containing a string password and an enum specifying the password type.
    /// Returning a null string in the first position of the tuple indicates that the user has cancelled
    /// the open operation.  Otherwise, the library will call GetPasswordAsync repeatedly until a working or
    /// null password is returned.
    /// </returns>
    ValueTask<(string?, PasswordType)> GetPasswordAsync();
}

[StaticSingleton()]
public partial class NullPasswordSource : IPasswordSource
{
    public ValueTask<(string?, PasswordType)> GetPasswordAsync() =>
        throw new InvalidDataException(
            "This file is encrypted.  To open encrypted files pass an IPasswordSource to the " +
            "PdfLowLevelReader or PdfReader constructor.");
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

    public ValueTask<(string?, PasswordType)> GetPasswordAsync() => new((TryGetNextPassword(), type));
    private string? TryGetNextPassword() => HasNextPassword() ? NextPassword() : null;
    private string? NextPassword() => passwords[next++];
    private bool HasNextPassword() => next < passwords.Length;
}