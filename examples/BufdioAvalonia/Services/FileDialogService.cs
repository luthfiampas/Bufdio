using System.Collections.Generic;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;

namespace BufdioAvalonia.Services;

public class FileDialogService : IFileDialogService
{
    public async Task<string> OpenAsync()
    {
        var dialog = new OpenFileDialog { Filters = new List<FileDialogFilter>(), AllowMultiple = false };

        if (Application.Current.ApplicationLifetime is ClassicDesktopStyleApplicationLifetime app)
        {
            var result = await dialog.ShowAsync(app.MainWindow);
            return result is { Length: >= 1 } ? result[0] : null;
        }

        return null;
    }
}
