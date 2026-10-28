# Native_Wallpaper
Have you ever wanted to have an animated wallpaper on windows without worrying about the always on functions (or potential data privacy concerns) of third party apps. This guide shows you how to make your own wallpaper function all you need is your own /mp4 file. 

# Animated Wallpaper for Windows 11

A lightweight animated desktop wallpaper built with C#, Windows Forms, and WebView2. Plays a looping MP4 video behind desktop icons and open application windows.

## Requirements

- Windows 11
- [.NET SDK](https://dotnet.microsoft.com/download)
- [Microsoft Edge WebView2 Runtime](https://developer.microsoft.com/microsoft-edge/webview2/)
- An MP4 video file

## Setup

**1. Clone the repository**

```bash
git clone https://github.com/YOUR-USERNAME/animated-wallpaper.git
cd animated-wallpaper
```

**2. Add your wallpaper video**

Place your video at:

```text
%USERPROFILE%\Videos\wallpaper.mp4
```

**3. Build the project**

```bash
dotnet build
```

**4. Run the wallpaper**

```bash
dotnet run
```

Your video should play behind your desktop icons and normal application windows.

## Configuration

Replace `wallpaper.mp4` in your Videos folder with your preferred video, keeping the same filename.

## Notes

- Tested on Windows 11 25H2 (OS Build 26200.9711).
- Uses Windows desktop window internals to position the wallpaper.
- Desktop attachment may behave differently across Windows versions.
- The current implementation targets the primary monitor.

## Technologies

- C#
- .NET
- Windows Forms
- Microsoft WebView2
- Windows User32 API
