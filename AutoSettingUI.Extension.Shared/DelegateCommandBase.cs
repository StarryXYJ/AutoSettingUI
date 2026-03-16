using System.Windows.Input;

namespace AutoSettingUI.Extension.Shared;

/// <summary>
/// Base implementation of a delegate command that supports explicit CanExecute notifications.
/// </summary>
public abstract class DelegateCommandBase : ICommand
{
    protected readonly Action _execute;
    protected readonly Func<bool>? _canExecute;

    public event EventHandler? CanExecuteChanged;

    protected DelegateCommandBase(Action execute, Func<bool>? canExecute = null)
    {
        _execute = execute ?? throw new ArgumentNullException(nameof(execute));
        _canExecute = canExecute;
    }

    public bool CanExecute(object? parameter) => _canExecute?.Invoke() ?? true;

    public void Execute(object? parameter) => _execute();

    /// <summary>
    /// Raises CanExecuteChanged to immediately notify subscribers.
    /// </summary>
    public void RaiseCanExecuteChanged()
    {
        CanExecuteChanged?.Invoke(this, EventArgs.Empty);
    }
}
