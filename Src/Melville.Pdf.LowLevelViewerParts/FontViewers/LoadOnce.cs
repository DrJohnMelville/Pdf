using System.Runtime.CompilerServices;
using Melville.INPC;

namespace Melville.Pdf.LowLevelViewerParts.FontViewers;

internal class LoadOnce<T>(T defaultValue) where T: class
{
    private T? value;

    public T GetValue(IExternalNotifyPropertyChanged parent, Func<ValueTask<T>> method, [CallerMemberName] string member = "")
    {
        if (value is null)
        {
            value = defaultValue;
            RunAsyncLoader(parent, method, member);
        }

        return value;
    }

    private async void RunAsyncLoader(IExternalNotifyPropertyChanged parent, Func<ValueTask<T>> method, string member)
    {
        value = await method();
        parent.OnPropertyChanged(member);
    }
}