using System;
using System.Windows.Input;
using Avalonia.Threading;

namespace BufdioAvalonia.Framework;

public abstract class DelegateCommandBase : ICommand
{
    public event EventHandler CanExecuteChanged;

    public abstract bool CanExecute(object parameter);

    public abstract void Execute(object parameter);

    public virtual void RaiseCanExecuteChanged()
    {
        var handler = CanExecuteChanged;
        Dispatcher.UIThread.InvokeAsync(() =>
        {
            handler?.Invoke(this, EventArgs.Empty);
        });
    }
}
