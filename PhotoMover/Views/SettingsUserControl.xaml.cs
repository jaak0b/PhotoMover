using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using Domain;
using Microsoft.Win32;

namespace PhotoMover.Views
{
  public sealed partial class SettingsUserControl : UserControl, INotifyPropertyChanged
  {
    public IAppConfig AppConfig { get; }

    public SettingsUserControl(IAppConfig appConfig)
    {
      AppConfig = appConfig;
      InitializeComponent();
      DataContext = AppConfig;
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    private void SelectFolderSourceButton_OnClick(object sender, RoutedEventArgs e)
    {
      OpenFolderDialog folderDialog = new()
                                      {
                                        Title = "Select Source Folder",
                                        Multiselect = false
                                      };

      if (folderDialog.ShowDialog() == true)
      {
        string? folderName = folderDialog.FolderName;
        AppConfig.FolderSource = folderName;
        OnPropertyChanged(null);
      }
    }

    private void SelectFolderTargetButton_OnClick(object sender, RoutedEventArgs e)
    {
      OpenFolderDialog folderDialog = new()
                                      {
                                        Title = "Select Target Folder",
                                        Multiselect = false
                                      };

      if (folderDialog.ShowDialog() == true)
      {
        string? folderName = folderDialog.FolderName;
        AppConfig.FolderTarget = folderName;
        OnPropertyChanged(null);
      }
    }

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
      PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
  }
}