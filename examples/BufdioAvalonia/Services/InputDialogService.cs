using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using BufdioAvalonia.Views;

namespace BufdioAvalonia.Services
{
    public class InputDialogService : IInputDialogService
    {
        public async Task<string> OpenAsync(string title = "", string description = "")
        {
            var dialog = new InputDialog();
            dialog.SetParameters(title, description);
            
            if (Application.Current.ApplicationLifetime is ClassicDesktopStyleApplicationLifetime app)
            {
                var result = await dialog.ShowDialog<string>(app.MainWindow);
                return result;
            }

            return null;
        }
    }
}
