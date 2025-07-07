using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace AfbeeldingenUitzoeken.Views
{
    public partial class KeepSubfolderPopup : Window
    {
        public string? SelectedSubfolder { get; private set; }

        public KeepSubfolderPopup(IEnumerable<string> subfolders)
        {
            InitializeComponent();
            _subfolders = new List<string>(subfolders);
            Loaded += (s, e) =>
            {
                RefreshButtons();
                AdjustWindowHeight();
            };
        }

        private readonly List<string> _subfolders = new();

        private void RefreshButtons()
        {
            ButtonsPanel.Items.Clear();
            if (FindName("ButtonsPanel") is System.Windows.Controls.ListBox listBox)
            {
                foreach (var subfolder in _subfolders)
                {
                    var item = new ListBoxItem
                    {
                        Content = subfolder,
                        Margin = new Thickness(2),
                    };
                    item.MouseDoubleClick += (s2, e2) => { SelectedSubfolder = subfolder; DialogResult = true; };
                    listBox.Items.Add(item);
                }
            }
        }

        private void AdjustWindowHeight()
        {
            if (FindName("ButtonsPanel") is System.Windows.Controls.ListBox listBox)
            {
                // Each item is about 28px + margin, plus header and button
                int itemCount = _subfolders.Count;
                double itemHeight = 28 + 4; // estimated ListBoxItem height + margin
                double baseHeight = 16 + 24 + 8 + 36 + 48 + 32; // margins, header, button, padding
                double listHeight = Math.Max(40, itemCount * itemHeight);
                this.Height = baseHeight + listHeight;
            }
        }

        private void ManageSubfolders_Click(object sender, RoutedEventArgs e)
        {
            // Open subfolder management dialog
            var vm = new ViewModels.KeepSubfolderViewModel(App.Current.MainWindow is MainWindow mw && mw.DataContext is ViewModels.MainViewModel mainVM ? mainVM.KeepFolderPath : string.Empty);
            var dialog = new ManageSubfoldersDialog(vm);
            dialog.Owner = this;
            dialog.ShowDialog();
            // After closing, reload subfolders in popup
            _subfolders.Clear();
            _subfolders.AddRange(vm.Subfolders);
            RefreshButtons();
        }
    }
}
