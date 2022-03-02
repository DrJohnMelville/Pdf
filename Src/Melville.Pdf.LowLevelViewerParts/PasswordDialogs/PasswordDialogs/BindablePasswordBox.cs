using System.Windows;
using System.Windows.Controls;

namespace Melville.Pdf.LowLevelViewerParts.PasswordDialogs.PasswordDialogs;

public class BindablePasswordBox : Decorator
{
    public static readonly DependencyProperty PasswordProperty;

    private bool isPreventCallback;
    private RoutedEventHandler savedCallback;

    static BindablePasswordBox()
    {
        PasswordProperty = DependencyProperty.Register(
            "Password",
            typeof(string),
            typeof(BindablePasswordBox),
            new FrameworkPropertyMetadata("", FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, new PropertyChangedCallback(OnPasswordPropertyChanged))
        );
    }

    public BindablePasswordBox()
    {
        savedCallback = HandlePasswordChanged;

        PasswordBox passwordBox = new PasswordBox();
        passwordBox.PasswordChanged += savedCallback;
        Child = passwordBox;
    }

    public string Password
    {
        get => (string) GetValue(PasswordProperty);
        set => SetValue(PasswordProperty, value);
    }

    private static void OnPasswordPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs eventArgs)
    {
        BindablePasswordBox bindablePasswordBox = (BindablePasswordBox) d;
        PasswordBox passwordBox = (PasswordBox) bindablePasswordBox.Child;

        if (bindablePasswordBox.isPreventCallback)
        {
            return;
        }

        passwordBox.PasswordChanged -= bindablePasswordBox.savedCallback;
        passwordBox.Password = (eventArgs.NewValue != null) ? eventArgs.NewValue.ToString() : "";
        passwordBox.PasswordChanged += bindablePasswordBox.savedCallback;
    }

    private void HandlePasswordChanged(object sender, RoutedEventArgs eventArgs)
    {
        PasswordBox passwordBox = (PasswordBox) sender;

        isPreventCallback = true;
        Password = passwordBox.Password;
        isPreventCallback = false;
    }
}