namespace Melville.Pdf.ComparingReader.MainWindow;

public interface ICommandLineSelection
{
    string? CommandLineTag();
}

public class CommandLineSelection: ICommandLineSelection
{
    private string[] commandLineArgs;

    public CommandLineSelection(string[] commandLineArgs)
    {
        this.commandLineArgs = commandLineArgs;
    }

    public string? CommandLineTag()
    {
        foreach (var arg in commandLineArgs)
        {
            if (arg.Length > 0 && arg[0] == '-') return arg[1..];
        }

        return null;
    }
}