using System;

namespace BufdioAvalonia.Framework;

public class DelegateCommand : DelegateCommandBase
{
    private readonly Action _execute;
    private readonly Func<bool> _canExecute;

    public DelegateCommand(Action execute, Func<bool> canExecute = null)
    {
        _execute = execute ?? throw new ArgumentNullException(nameof(execute));
        _canExecute = canExecute;
    }

    public override bool CanExecute(object parameter)
    {
        return _canExecute?.Invoke() ?? true;
    }

    public override void Execute(object parameter)
    {
        _execute();
    }
}
