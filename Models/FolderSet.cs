using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace AfbeeldingenUitzoeken.Models
{
    public class FolderSet
    {
        public string Name { get; set; } = string.Empty;
        public string LibraryPath { get; set; } = string.Empty;
        public string KeepFolderPath { get; set; } = string.Empty;
        public string BinFolderPath { get; set; } = string.Empty;
        public string CheckLaterFolderPath { get; set; } = string.Empty;

        public FolderSet() { }

        public FolderSet(string name, string libraryPath, string keepFolderPath, string binFolderPath, string checkLaterFolderPath)
        {
            Name = name;
            LibraryPath = libraryPath;
            KeepFolderPath = keepFolderPath;
            BinFolderPath = binFolderPath;
            CheckLaterFolderPath = checkLaterFolderPath;
        }

        /// <summary>
        /// Validates if all folder paths in the set exist
        /// </summary>
        /// <returns>Dictionary of path keys and validation results (true if valid, false if invalid)</returns>
        public Dictionary<string, bool> ValidatePaths()
        {
            return new Dictionary<string, bool>
            {
                { "LibraryPath", string.IsNullOrEmpty(LibraryPath) || Directory.Exists(LibraryPath) },
                { "KeepFolderPath", string.IsNullOrEmpty(KeepFolderPath) || Directory.Exists(KeepFolderPath) },
                { "BinFolderPath", string.IsNullOrEmpty(BinFolderPath) || Directory.Exists(BinFolderPath) },
                { "CheckLaterFolderPath", string.IsNullOrEmpty(CheckLaterFolderPath) || Directory.Exists(CheckLaterFolderPath) }
            };
        }

        /// <summary>
        /// Checks if any path in the set is invalid
        /// </summary>
        /// <returns>True if at least one path is invalid, false if all paths are valid</returns>
        public bool HasInvalidPaths()
        {
            var validationResults = ValidatePaths();
            return validationResults.Values.Any(isValid => !isValid);
        }

        /// <summary>
        /// Provides a string representation of the folder set
        /// </summary>
        /// <returns>The name of the folder set</returns>
        public override string ToString()
        {
            return Name;
        }
    }
}
