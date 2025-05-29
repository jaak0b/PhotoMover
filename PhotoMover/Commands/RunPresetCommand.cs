using System.IO;
using System.Windows.Input;
using CommonServiceLocator;
using Domain;
using Domain.Model;
using Domain.Service;
using Microsoft.Extensions.DependencyInjection;

namespace PhotoMover.Commands;

public class RunPresetCommand : ICommand
{
    private readonly ITaskService _taskService = ServiceLocator.Current.GetRequiredService<ITaskService>();

    public bool CanExecute(object? parameter)
    {
        return parameter is PresetModel preset && Directory.Exists(preset.SourceFolder) &&
               Directory.Exists(preset.DestinationFolder);
    }

    public async void Execute(object? parameter)
    {
        await Task.Run(
            () =>
            {
                if (parameter is not PresetModel preset)
                    return;
                _taskService.LoadFiles(preset, TaskType.CreatedByUser);
                _taskService.ProcessTasks();
                _taskService.FinalizeTask();
            });
    }

    public event EventHandler? CanExecuteChanged;
}