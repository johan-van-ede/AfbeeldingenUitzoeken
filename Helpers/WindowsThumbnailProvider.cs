using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media.Imaging;

namespace AfbeeldingenUitzoeken.Helpers
{
    /// <summary>
    /// Provides access to the Windows Shell thumbnail cache
    /// </summary>
    public static class WindowsThumbnailProvider
    {
        [DllImport("shell32.dll", CharSet = CharSet.Unicode)]
        private static extern int SHCreateItemFromParsingName(
            [MarshalAs(UnmanagedType.LPWStr)] string path,
            IntPtr pbc, 
            Guid riid, 
            out IShellItem shellItem);

        [DllImport("gdi32.dll")]
        private static extern bool DeleteObject(IntPtr hObject);

        [ComImport]
        [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        [Guid("43826d1e-e718-42ee-bc55-a1e261c37bfe")]
        private interface IShellItem
        {
            void MethodA();
            void MethodB();
            void GetDisplayName(int sigdnName, out IntPtr ppszName);
        }

        [ComImport]
        [Guid("bcc18b79-ba16-442f-80c4-8a59c30c463b")]
        [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        private interface IShellItemImageFactory
        {
            [PreserveSig]
            int GetImage(
                [In, MarshalAs(UnmanagedType.Struct)] SIZE size,
                [In] ThumbnailOptions flags,
                [Out] out IntPtr phbm);
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct SIZE
        {
            public int cx;
            public int cy;

            public SIZE(int cx, int cy)
            {
                this.cx = cx;
                this.cy = cy;
            }
        }

        private enum ThumbnailOptions : uint
        {
            ResizeToFit = 0x00,
            BiggerSizeOk = 0x01,
            InMemoryOnly = 0x02,
            IconOnly = 0x04,
            ThumbnailOnly = 0x08,
            InCacheOnly = 0x10,
            IconBackground = 0x80,
            ScaleUp = 0x100
        }

        /// <summary>
        /// Gets a thumbnail for a file using the Windows Shell
        /// </summary>
        /// <param name="filePath">The path to the file</param>
        /// <param name="width">The desired width of the thumbnail</param>
        /// <param name="height">The desired height of the thumbnail</param>        /// <returns>A BitmapSource containing the thumbnail, or null if it couldn't be retrieved</returns>
        public static BitmapSource? GetThumbnail(string filePath, int width, int height)
        {
            if (!File.Exists(filePath))
                return null;

            IShellItem shellItem = null;
            IntPtr hBitmap = IntPtr.Zero;

            try
            {
                Guid shellItemGuid = new Guid("43826d1e-e718-42ee-bc55-a1e261c37bfe");
                int hr = SHCreateItemFromParsingName(filePath, IntPtr.Zero, shellItemGuid, out shellItem);
                  if (hr != 0)
                    return null;

                IShellItemImageFactory? shellItemImageFactory = (IShellItemImageFactory?)shellItem;
                
                SIZE size = new SIZE(width, height);
                ThumbnailOptions options = ThumbnailOptions.ThumbnailOnly | ThumbnailOptions.BiggerSizeOk;
                
                hr = shellItemImageFactory.GetImage(size, options, out hBitmap);
                
                if (hr != 0 || hBitmap == IntPtr.Zero)
                    return null;

                // Convert HBitmap to BitmapSource
                BitmapSource bitmapSource = Imaging.CreateBitmapSourceFromHBitmap(
                    hBitmap,
                    IntPtr.Zero,
                    Int32Rect.Empty,
                    BitmapSizeOptions.FromEmptyOptions());

                bitmapSource.Freeze(); // Make it usable across threads
                
                return bitmapSource;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error getting Windows thumbnail: {ex.Message}");
                return null;
            }
            finally
            {
                // Clean up resources
                if (shellItem != null)
                    Marshal.ReleaseComObject(shellItem);
                
                if (hBitmap != IntPtr.Zero)
                    DeleteObject(hBitmap);
            }
        }
    }
}
