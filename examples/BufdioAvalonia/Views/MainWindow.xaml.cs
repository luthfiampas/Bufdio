using System.ComponentModel;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using BufdioAvalonia.ViewModels;

namespace BufdioAvalonia.Views
{
    public class MainWindow : Window
    {
        private readonly ProgressBar _bar;
        
        public MainWindow()
        {
            AvaloniaXamlLoader.Load(this);

            _bar = this.Find<ProgressBar>("ProgressBar");
            _bar.PointerReleased += OnProgressBarReleased; ;
        }
        
        private void OnProgressBarReleased(object sender, PointerReleasedEventArgs e)
        {
            if (DataContext is not MainWindowViewModel vm)
            {
                return;
            }
            
            var ratio = e.GetPosition(_bar).X / _bar.Bounds.Width;
            var value = ratio * _bar.Maximum;
            
            _bar.Value = value;
            vm.Seek(value);
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            if (DataContext is MainWindowViewModel vm)
            {
                vm.DisposePlayer();
            }
            
            base.OnClosing(e);
        }
    }
}
