using Melville.FileSystem;

namespace Melville.Pdf.ComparingReader.MainWindow;

public interface ICommandLineSelection
{
    string? CommandLineTag();
    IFile? CmdLineFile();
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

    public IFile? CmdLineFile() =>
        commandLineArgs.Length > 0 &&
        new FileSystemFile(commandLineArgs[0]) is { } ret &&
        ret.Exists()
            ? ret
            : null;
}