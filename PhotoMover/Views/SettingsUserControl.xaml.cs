using System.Windows.Controls;
using Domain;

namespace PhotoMover.Views
{
  public partial class SettingsUserControl : UserControl
  {
    public IAppConfig AppConfig { get; }

    public SettingsUserControl(IAppConfig appConfig)
    {
      AppConfig = appConfig;
      InitializeComponent();
    }
  }
}