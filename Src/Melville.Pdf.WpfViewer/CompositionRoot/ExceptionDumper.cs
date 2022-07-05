using System;
using System.IO;
using System.Linq;
using System.Text;
using Melville.Linq;

namespace Melville.Pdf.WpfViewer.CompositionRoot;

public static class ExceptionDumper
{
    public static void RegisterExceptionDumper()
    {
#if !DEBUG
      AppDomain.CurrentDomain.UnhandledException += HandledException;
#endif
    }
    static void HandledException(object sender, UnhandledExceptionEventArgs e)
    {
        File.WriteAllText(ExceptionLogPath(), WriteException(e.ExceptionObject), Encoding.UTF8);
    }
    private static string ExceptionLogPath()
    {
        return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), 
            "Crash Dump.txt");
    }
    private static String WriteException(object exceptionObject)
    {
        var exception = exceptionObject as Exception;
        if (exception != null)
        {
            return exception.PrettyPrint();
        }
        return exceptionObject == null ? "Threw a null object." : 
            "Non-Exception object thrown:" + exceptionObject;
    }
}

public static class ExceptionExtension
{
    public static string PrettyPrint(this Exception exception) =>
        string.Join("--------------------\r\n", 
            FunctionalMethods.Sequence(exception, i => i.InnerException!).
                Reverse().
                Select(i => String.Format("{0}\n{1}\n{2}\n", i.GetType(), i.Message, i.StackTrace)));
}