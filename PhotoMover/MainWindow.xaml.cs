using System.Windows;
using PhotoMover.ViewModels;

namespace PhotoMover
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void InsertPlaceholder_Click(object sender, RoutedEventArgs e)
        {
            if (sender is System.Windows.Controls.Button button && 
                DataContext is MainWindowViewModel viewModel &&
                button.Content is string placeholder)
            {
                viewModel.RuleEditorViewModel.InsertPlaceholder(placeholder);
            }
        }
    }
}

