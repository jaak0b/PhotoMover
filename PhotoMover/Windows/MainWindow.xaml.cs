using System.Windows;
using PhotoMover.Views;

namespace PhotoMover.Windows
{
  public partial class MainWindow : Window
  {
    private readonly SettingsUserControl _settingsUserControl;

    public MainWindow(SettingsUserControl settingsUserControl)
    {
      _settingsUserControl = settingsUserControl;
      InitializeComponent();
    }

    private void MainWindow_OnInitialized(object? sender, EventArgs e)
    {
      SettingsTabItem.Content = _settingsUserControl;
    }
  }
}