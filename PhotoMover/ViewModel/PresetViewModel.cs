using Domain.Model;
using JetBrains.Annotations;

namespace PhotoMover.ViewModel;

public class PresetViewModel(PresetModel presetModel) : Core.ViewModel
{
    public string? Name
    {
        get => presetModel.Name;
        set
        {
            presetModel.Name = value;
            OnPropertyChanged();
        }
    }

    public string? SourceFolder
    {
        get => presetModel.SourceFolder;
        set
        {
            presetModel.SourceFolder = value;
            OnPropertyChanged();
        }
    }

    public string? DestinationFolder
    {
        get => presetModel.DestinationFolder;
        set
        {
            presetModel.DestinationFolder = value;
            OnPropertyChanged();
        }
    }

    public string FilePattern
    {
        get => presetModel.FilePattern;
        set
        {
            presetModel.FilePattern = value;
            OnPropertyChanged();
        }
    }

    public string? FolderPattern
    {
        get => presetModel.FolderPattern;
        set
        {
            presetModel.FolderPattern = value;
            OnPropertyChanged();
        }
    }
}