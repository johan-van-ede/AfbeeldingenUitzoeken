using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using AfbeeldingenUitzoeken.Models;
using AfbeeldingenUitzoeken.Views;
using AfbeeldingenUitzoeken.Helpers;
// Explicitly use Windows MessageBox and Application to avoid ambiguity
using MessageBox = System.Windows.MessageBox;
using Application = System.Windows.Application;

namespace AfbeeldingenUitzoeken.ViewModels
{
    public class VideoSupportViewModel : BaseMediaViewModel
    {
        // All the media-related properties and functionality are now inherited from BaseMediaViewModel
    }
}
