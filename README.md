# Animated Wallpaper for Windows 11

A custom animated desktop wallpaper built with C#, Windows Forms, and Microsoft WebView2. The application plays a looping MP4 video as the desktop background while keeping desktop icons and normal application windows available.

This project was built to run on Windows 11 without requiring Windows activation or a third-party wallpaper application.

## Features

- Plays a local MP4 video as an animated desktop background.
- Loops the video automatically.
- Uses C# and Windows Forms for the application.
- Uses Microsoft WebView2 to render the video.
- Uses Windows API calls to position the application behind desktop icons.
- Runs without Wallpaper Engine or other dedicated wallpaper software.
- Uses a local video file, so an internet connection is not required for playback after setup.

## How It Works

The application combines three components:

1. **C# and Windows Forms:** Create the application window and manage its lifecycle.
2. **Microsoft WebView2:** Renders an HTML video element that plays the MP4 file.
3. **Win32 API:** Interacts with Windows desktop windows to position the video behind the desktop icons.

The application creates a borderless window, loads an HTML page containing an autoplaying, muted, looping video, and then attempts to attach the window to a suitable Windows desktop layer.

The desktop integration uses functions such as `FindWindow`, `FindWindowEx`, `SendMessageTimeout`, `SetParent`, `SetWindowLongPtr`, and `SetWindowPos`.

Windows does not provide a guaranteed, stable public API for hosting arbitrary applications as desktop wallpaper. The `WorkerW` approach therefore depends on Windows' desktop window hierarchy and may behave differently across Windows versions and configurations.

## Requirements

- Windows 11
- .NET 10 SDK
- Microsoft WebView2 Runtime
- A local MP4 video file
- Command Prompt or another terminal

The WebView2 Runtime is required to render the video. It may already be installed on your computer.

## Project Setup

### 1. Install the .NET SDK

Install the .NET SDK from the official Microsoft website:

https://dotnet.microsoft.com/download/dotnet/10.0

Verify that it is installed by opening Command Prompt and running:

```cmd
dotnet --version
```

The command should return an installed SDK version.

### 2. Create the Windows Forms project

Navigate to the directory where you want to create the project.

Create the project using:

```cmd
dotnet new winforms -n AnimatedWallpaper
cd AnimatedWallpaper
```

This generates the basic Windows Forms application structure.

### 3. Install the WebView2 NuGet package

Run:

```cmd
dotnet add package Microsoft.Web.WebView2
```

This adds the WebView2 controls and APIs needed to render the HTML video player.

The WebView2 Runtime is separate from the NuGet package. The package provides the application integration, while the runtime provides the browser engine.

### 4. Add the wallpaper video

Place your video in the current Windows user's Videos folder and name it:

```text
wallpaper.mp4
```

The application locates the file using:

```csharp
Environment.GetFolderPath(
    Environment.SpecialFolder.MyVideos)
```

The expected path will typically resemble:

```text
C:\Users\<username>\Videos\wallpaper.mp4
```

To use a different video, either replace `wallpaper.mp4` or update the filename in `Form1.cs`.

### 5. Replace Form1.cs

Open `Form1.cs` and replace its contents with the implementation included in this repository.

The implementation is responsible for:

- Creating the borderless wallpaper window.
- Initializing WebView2.
- Mapping the Videos folder to a local virtual hostname.
- Loading the HTML video element.
- Locating a suitable Windows desktop layer.
- Reparenting and positioning the window.
- Handling missing video files and initialization errors.

The HTML video player uses a local virtual hostname:

```text
https://wallpaper.local/wallpaper.mp4
```

WebView2 maps this hostname to the user's Videos folder through `SetVirtualHostNameToFolderMapping`. This allows the HTML page to load the local video through WebView2's virtual-host mechanism.

### 6. Build the application

Run:

```cmd
dotnet build
```

Wait for the build to complete. Resolve any compilation errors before proceeding.

A `WindowsBase` version conflict warning may appear because of transitive references included with the WebView2 package. This warning is distinct from a compilation error, but it should be investigated if it leads to runtime problems.

### 7. Run the application

Run:

```cmd
dotnet run
```

The application checks that the video exists, initializes WebView2, loads the video, waits briefly for the browser's video surface to initialize, and then attempts desktop integration.

If everything works, the video should fill the desktop background while the desktop icons remain accessible.

## Windows Desktop Integration

### The WorkerW approach

Windows desktop rendering involves shell-managed windows, including `Progman`, `WorkerW`, and `SHELLDLL_DefView`.

The implementation sends the `0x052C` message to `Progman` to request the creation of a desktop worker window. It then searches for a suitable target and uses `SetParent` to reparent the wallpaper window.

The code includes multiple discovery paths because desktop window arrangements can differ across Windows configurations.

It also changes the window styles to make the application a child window, applies layered-window attributes, and positions the window using `SetWindowPos`.

### Why this required experimentation

Simply creating a borderless, fullscreen video window does not make it a wallpaper. A normal fullscreen window covers desktop icons and other windows, behaving like any other application.

The challenge was getting Windows to treat the video window as part of the desktop rather than as a regular foreground application.

The initial implementation used a basic `WorkerW` search. On the test system, that search did not find a suitable wallpaper layer. Subsequent testing explored alternative window discovery methods, parent selection, and window positioning.

The current implementation includes additional checks and fallback logic to handle these differences.

**Important:** The desktop integration is experimental. The relevant Windows API behavior is not guaranteed across all Windows 11 builds. If the video covers the screen, disappears, or attaches to the wrong window, terminate the application and review the window discovery logic.

## Troubleshooting

### The video does not play

Check that:

- The file exists in the user's Videos folder.
- Its name is exactly `wallpaper.mp4`.
- The file plays normally in a browser or media player.
- The WebView2 Runtime is installed.

### The build fails

Run:

```cmd
dotnet build
```

Review the compiler errors. A warning does not necessarily indicate that the build failed. Resolve errors before running the application.

### The video covers everything

A fullscreen video window alone is not sufficient for desktop integration. Verify that the desktop attachment succeeds and that the window is parented to the intended desktop layer.

If necessary, stop the application through Task Manager before making further changes.

### The desktop layer cannot be found

Windows may expose a different window arrangement than the implementation expects. The `WorkerW` technique is sensitive to desktop implementation details, so the search logic may need to be adapted for the affected Windows build.

### How to stop the application

If the wallpaper prevents you from accessing Command Prompt:

1. Press `Ctrl + Shift + Esc` to open Task Manager.
2. Find `AnimatedWallpaper` in the process list.
3. Select **End task**.

If you launched the application using `dotnet run`, stopping the application also ends that run. Avoid terminating all `dotnet.exe` processes unless you intend to stop other .NET applications too.

## Development Notes

This project was developed incrementally:

1. Created a Windows Forms application using the .NET SDK.
2. Added Microsoft WebView2.
3. Verified that the WebView2 Runtime was available.
4. Tested local MP4 playback in a regular application window.
5. Added HTML video playback with autoplay, muted audio, and looping.
6. Investigated the Windows desktop window hierarchy.
7. Implemented desktop window discovery and reparenting through Win32 API calls.
8. Tested window styles, parenting, and positioning to integrate the video with the desktop.

The ordinary video-player implementation was retained as a backup during development so that desktop integration experiments would not destroy the working playback implementation.

## Limitations

- The current implementation targets a single monitor.
- The video is expected to be located in the current user's Videos folder.
- The project uses Windows-specific APIs and is not cross-platform.
- Desktop integration may require changes for different Windows versions or configurations.
- The application does not currently provide a settings interface for selecting videos.
- Automatic startup and multi-monitor support are not included in the current implementation.

## Planned Future Improvements ( hopefully I remember this project :) )

- Support for multiple monitors.
- Automatic startup after user sign-in.
- Better recovery if the Windows desktop restarts.
- More robust detection of the correct desktop window (larger applicability window for different windows versions) 
- Configuration for video scaling and playback behavior.

## License

MIT License.

