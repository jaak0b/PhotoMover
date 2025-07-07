using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using Domain;
using Domain.Model;
using Domain.Service;
using Microsoft.EntityFrameworkCore;
using PhotoMover.Views;

namespace PhotoMover.Windows
{
  public partial class MainWindow : Window, INotifyPropertyChanged
  {
    private bool _showOverview;
    private bool _showLoadFilesProgress;
    private bool _processFilesProgress;
    private bool _moveFilesProgress;
    private DateTime _lastUpdate = DateTime.Now;
    private readonly SettingsUserControl _settingsUserControl;
    private readonly IFileMoverService _fileMoverService;
    private readonly IAppConfig _appConfig;

    private Database Database { get; }

    public bool ShowOverview
    {
      get => _showOverview;
      set
      {
        _showOverview = value;
        OnPropertyChanged();
      }
    }

    public bool ShowLoadFilesProgress
    {
      get => _showLoadFilesProgress;
      set
      {
        _showLoadFilesProgress = value;
        OnPropertyChanged();
      }
    }

    public bool ProcessFilesProgress
    {
      get => _processFilesProgress;
      set
      {
        _processFilesProgress = value;
        OnPropertyChanged();
      }
    }

    public bool MoveFilesProgress
    {
      get => _moveFilesProgress;
      set
      {
        _moveFilesProgress = value;
        OnPropertyChanged();
      }
    }

    public MainWindow(SettingsUserControl settingsUserControl, Database database, IFileMoverService fileMoverService,
                      IAppConfig appConfig)
    {
      Database = database;
      _settingsUserControl = settingsUserControl;
      _fileMoverService = fileMoverService;
      _appConfig = appConfig;
      InitializeComponent();
    }

    private void MainWindow_OnInitialized(object? sender, EventArgs e)
    {
      SettingsTabItem.Content = _settingsUserControl;
      TasksDataGrid.ItemsSource = Database.Tasks.Local.ToObservableCollection();
      Database.CollectionChanged += Database_OnCollectionChanged;
    }

    private void Database_OnCollectionChanged(object? sender, EventArgs e)
    {
      if (DateTime.Now - _lastUpdate > TimeSpan.FromSeconds(2))
      {
        _lastUpdate = DateTime.Now;
        OnPropertyChanged(null);

        Dispatcher.Invoke(
                          () =>
                          {
                            List<TaskModel> files = Database.Tasks.AsNoTracking().ToList();
                            ProcessFilesProgressBar.Maximum = files.Count;
                            MoveFilesProgressBar.Maximum = files.Count;
                            ProcessFilesProgressBar.Value =
                              files.Count(taskModel => taskModel.State is State.Processed);
                            MoveFilesProgressBar.Value =
                              files.Count(taskModel => taskModel.State is State.Moved or State.Error);
                          });
      }
    }

    private async void ButtonBase_OnClick(object sender, RoutedEventArgs e)
    {
      try
      {
        ProcessFilesButton.IsEnabled = false;

        ShowLoadFilesProgress = false;
        ProcessFilesProgress = false;
        MoveFilesProgress = false;

        ShowLoadFilesProgress = true;
        await Database.Tasks.ExecuteDeleteAsync();
        await Task.Run(() => _fileMoverService.LoadFiles(_appConfig.FolderSource, TaskType.CreatedByUser));

        ShowLoadFilesProgress = false;
        ProcessFilesProgress = true;
        await Task.Run(() => _fileMoverService.ProcessFiles());

        MoveFilesProgress = true;
        await Task.Run(() => _fileMoverService.MoveFiles());

        TasksDataGrid.ItemsSource = Database.Tasks.AsNoTracking().ToList();
        ShowOverview = true;
      }
      finally
      {
        ShowLoadFilesProgress = false;
        ProcessFilesProgress = false;
        MoveFilesProgress = false;

        ProcessFilesButton.IsEnabled = true;
      }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
      PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
  }
}