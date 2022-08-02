using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace MeasurementApp.Services;

public static class ModalView
{
    public static async Task MessageDialogAsync(this FrameworkElement element, string title, string message)
    {
        await MessageDialogAsync(element, title, message, "OK");
    }

    public static async Task MessageDialogAsync(this FrameworkElement element, string title, string message, string buttonText)
    {
        var dialog = new ContentDialog
        {
            Title = title,
            Content = message,
            CloseButtonText = buttonText,
            XamlRoot = element.XamlRoot,
            RequestedTheme = element.ActualTheme
        };

        await dialog.ShowAsync();
    }

    public static async Task<bool?> ConfirmationDialogAsync(this FrameworkElement element, string title)
    {
        return await ConfirmationDialogAsync(element, title, "OK", string.Empty, "Cancel");
    }

    public static async Task<bool?> ConfirmationDialogAsync(this FrameworkElement element, string title, string yesButtonText, string noButtonText)
    {
        return await ConfirmationDialogAsync(element, title, yesButtonText, noButtonText, string.Empty);
    }

    public static async Task<bool?> ConfirmationDialogAsync(this FrameworkElement element, string title, string yesButtonText, string noButtonText, string cancelButtonText)
    {
        var dialog = new ContentDialog
        {
            Title = title,
            PrimaryButtonText = yesButtonText,
            SecondaryButtonText = noButtonText,
            CloseButtonText = cancelButtonText,
            XamlRoot = element.XamlRoot,
            RequestedTheme = element.ActualTheme
        };

        var result = await dialog.ShowAsync();

        if(result == ContentDialogResult.None)
        {
            return null;
        }

        return (result == ContentDialogResult.Primary);
    }
}
