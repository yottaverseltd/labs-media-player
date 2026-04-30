using Android.App;
using Android.Runtime;
using System;

namespace LabsMediaPlayer.Droid;

[Application(
    Label = "@string/ApplicationName",
    Icon = "@mipmap/icon",
    LargeHeap = true,
    HardwareAccelerated = true,
    Theme = "@style/AppTheme",
    UsesCleartextTraffic = true)]
public class Application : Microsoft.UI.Xaml.NativeApplication
{
    public Application(IntPtr javaReference, JniHandleOwnership transfer)
        : base(() => new global::LabsMediaPlayer.App(), javaReference, transfer)
    {
    }
}
