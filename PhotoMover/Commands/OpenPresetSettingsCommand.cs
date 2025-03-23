using System.Windows.Input;

namespace PhotoMover.Commands;

public class OpenPresetSettingsCommand : ICommand
{
    public bool CanExecute(object? parameter) => true;

    public void Execute(object? parameter)
    {
    }

    public event EventHandler? CanExecuteChanged;
}