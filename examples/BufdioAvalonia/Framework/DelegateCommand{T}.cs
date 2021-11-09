using System;

namespace BufdioAvalonia.Framework;

public class DelegateCommand<T> : DelegateCommandBase
{
    private readonly Action<T> _execute;
    private readonly Func<T, bool> _canExecute;

    public DelegateCommand(Action<T> execute, Func<T, bool> canExecute = null)
    {
        _execute = execute ?? throw new ArgumentNullException(nameof(execute));
        _canExecute = canExecute;
    }

    public override bool CanExecute(object parameter)
    {
        return _canExecute?.Invoke((T)parameter) ?? true;
    }

    public override void Execute(object parameter)
    {
        _execute((T)parameter);
    }
}
