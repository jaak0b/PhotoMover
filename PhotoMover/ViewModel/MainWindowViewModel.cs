using System.Collections.ObjectModel;
using Autofac.Core;
using CommonServiceLocator;
using Domain;
using Domain.Model;
using Microsoft.Extensions.DependencyInjection;
using PhotoMover.Commands;

namespace PhotoMover.ViewModel;

public class MainWindowViewModel : Core.ViewModel
{
    private readonly Database _db;

    public MainWindowViewModel()
    {
        _db = ServiceLocator.Current.GetRequiredService<Database>();
        _db.CollectionChanged += Db_OnCollectionChanged;
        OpenPresetSettingsCommand = ServiceLocator.Current.GetRequiredService<OpenPresetSettingsCommand>();
    }

    private void Db_OnCollectionChanged(object? sender, EventArgs e)
    {
        OnPropertyChanged(null);
    }

    public ObservableCollection<PresetModel> Presets => new(_db.Presets.Select(e => e ));
    
    public OpenPresetSettingsCommand OpenPresetSettingsCommand { get; }
}