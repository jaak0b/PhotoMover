using System.Net;
using System.Windows;
using System.Windows.Controls;
using CommonServiceLocator;
using Domain;
using Domain.Model;
using Domain.Service;
using Microsoft.Extensions.DependencyInjection;

namespace PhotoMover.UserControls;

public partial class SettingsUserControl
{
    public FtpConfigurationModel FtpConfiguration { get; set; }

    public Database Database { get; set; }

    public SettingsUserControl()
    {
        FtpConfiguration = ServiceLocator.Current.GetInstance<IFtpConfigurationService>().GetFtpConfigurationModel();
        Database = ServiceLocator.Current.GetRequiredService<Database>();
        InitializeComponent();
        this.DataContext = FtpConfiguration;
    }

    private void ToggleButton_OnChecked(object sender, RoutedEventArgs e) => Database.Update(FtpConfiguration);

    private void TextBoxBase_OnTextChanged(object sender, TextChangedEventArgs e) => Database.Update(FtpConfiguration);
}