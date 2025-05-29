using System.Windows.Input;
using CommonServiceLocator;
using Domain;
using Domain.Model;
using Microsoft.Extensions.DependencyInjection;

namespace PhotoMover.Commands;

public class AddPresetCommand : ICommand
{
    Database _db = ServiceLocator.Current.GetRequiredService<Database>();
    public bool CanExecute(object? parameter) => true;

    public void Execute(object? parameter)
    {
        _db.Add(new PresetModel());
    }

    public event EventHandler? CanExecuteChanged;
}