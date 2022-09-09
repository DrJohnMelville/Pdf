using System.Threading.Tasks;
using Melville.Pdf.LowLevel.Parsing.ParserContext;

namespace Melville.Pdf.ComparingReader.MainWindow;

public class PasswordBox : IPasswordSource
{
    public string Password { get; set; } = "";
    public PasswordType Type { get; set; }
    
    public ValueTask<(string?, PasswordType)> GetPasswordAsync()
    {
        return new((Password, Type));
    }
}