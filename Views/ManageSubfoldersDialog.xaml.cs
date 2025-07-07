using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using AfbeeldingenUitzoeken.ViewModels;

namespace AfbeeldingenUitzoeken.Views
{
    public partial class ManageSubfoldersDialog : Window
    {
        private readonly KeepSubfolderViewModel _viewModel;
        public ManageSubfoldersDialog(KeepSubfolderViewModel viewModel)
        {
            InitializeComponent();
            _viewModel = viewModel;
            SubfoldersListBox.ItemsSource = _viewModel.Subfolders;
        }

        private void AddSubfolder_Click(object sender, RoutedEventArgs e)
        {
            var name = NewSubfolderTextBox.Text.Trim();
            if (!string.IsNullOrEmpty(name))
            {
                _viewModel.AddSubfolder(name);
                NewSubfolderTextBox.Text = string.Empty;
            }
        }

        private void RemoveSubfolder_Click(object sender, RoutedEventArgs e)
        {
            if (SubfoldersListBox.SelectedItem is string name)
            {
                _viewModel.RemoveSubfolder(name);
            }
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }
    }
}
