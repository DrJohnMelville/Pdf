using System.Threading.Tasks;
using Melville.Pdf.LowLevel;
using Melville.Pdf.LowLevel.Parsing.ParserContext;

namespace Performance.Playground.Encryption;

public class LoadV6File
{
    public async Task TestParser()
    {
        var reader = await new PdfLowLevelReader(new ConstantPasswordSource(PasswordType.User, "User"))
            .ReadFromFile(@"C:\Users\jmelv\Documents\Scratch\SodaPDF-protected-camp form.pdf");
    }
}