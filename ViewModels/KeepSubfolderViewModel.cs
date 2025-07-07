using System.Collections.ObjectModel;
using System.IO;
using System.Linq;

namespace AfbeeldingenUitzoeken.ViewModels
{
    public class KeepSubfolderViewModel
    {
        public ObservableCollection<string> Subfolders { get; set; } = new();
        public string KeepFolderPath { get; set; } = string.Empty;

        public KeepSubfolderViewModel(string keepFolderPath)
        {
            KeepFolderPath = keepFolderPath;
            LoadSubfolders();
        }

        public void LoadSubfolders()
        {
            Subfolders.Clear();
            if (!string.IsNullOrEmpty(KeepFolderPath) && Directory.Exists(KeepFolderPath))
            {
                var subfolders = Directory.GetDirectories(KeepFolderPath)
                    .Select(Path.GetFileName)
                    .Where(name => !string.IsNullOrEmpty(name))
                    .ToList();
                foreach (var sub in subfolders)
                    Subfolders.Add(sub!);
            }
        }

        public void AddSubfolder(string name)
        {
            if (string.IsNullOrWhiteSpace(name)) return;
            var path = Path.Combine(KeepFolderPath, name);
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
            if (!Subfolders.Contains(name))
                Subfolders.Add(name);
        }

        public void RemoveSubfolder(string name)
        {
            var path = Path.Combine(KeepFolderPath, name);
            if (Directory.Exists(path))
                Directory.Delete(path, true);
            Subfolders.Remove(name);
        }
    }
}
