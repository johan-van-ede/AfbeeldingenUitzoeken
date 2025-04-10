using System;
using System.IO;
using System.Reflection;

namespace AfbeeldingenUitzoeken.Models
{    public static class VersionInfo
    {
        // Version format: Major.Minor.Build.Revision
        private static readonly Version _version = new Version(1, 0, 1, 0); // Version 1.0.1 release
        
        // Make current version accessible for version comparison
        public static Version CurrentVersion => _version;
        
        public static string VersionString => $"v{_version.Major}.{_version.Minor}.{_version.Build}.{_version.Revision}";
        
        public static string VersionWithLabel => $"{VersionString}";
    }
}
