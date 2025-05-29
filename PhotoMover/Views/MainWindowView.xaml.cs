using System.Windows;
using System.Windows.Controls;
using CommonServiceLocator;
using Domain;
using Domain.Model;
using Microsoft.Extensions.DependencyInjection;

namespace PhotoMover.Views;

public partial class MainWindowView : UserControl
{
    public MainWindowView()
    {
        InitializeComponent();
    }

    private void MenuItem_OnClick(object sender, RoutedEventArgs e)
    {
       PresetModel? model =  (((sender as MenuItem)?.Parent as ContextMenu)?.PlacementTarget as FrameworkElement)?.DataContext as PresetModel;
       Database db = ServiceLocator.Current.GetRequiredService<Database>();
       if(model != null)
        db.Remove(model);
    }
}