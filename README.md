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
- Runs on Startup

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

Install the .NET 10 SDK from Microsoft's official website:

https://dotnet.microsoft.com/download/dotnet/10.0

Under SDK 10.0.403, select:
- OS: Windows
- Architecture: x64
- Download: Installer

Verify the installation by opening Command Prompt and running:

```cmd
dotnet --version
```

### 2. Create the project

Run the following commands in Command Prompt:

```cmd
dotnet new winforms -n AnimatedWallpaper
cd AnimatedWallpaper
```

This creates a new Windows Forms application and generates the project configuration file (`AnimatedWallpaper.csproj`) along with the standard source files.

You do not need to download a prebuilt project configuration file from this repository. The .NET SDK generates one for your environment.

### 3. Install WebView2

Add the Microsoft WebView2 NuGet package:

```cmd
dotnet add package Microsoft.Web.WebView2
```

This package allows the application to embed the WebView2 browser control.

The WebView2 Runtime is also required. It may already be installed on your system.

### 4. Replace Form1.cs

Open the newly created `AnimatedWallpaper` project folder in file explorer.

Replace the contents of the generated `Form1.cs` file with the `Form1.cs` implementation provided in this repository. Or simply cut and paste the file from downloads into the folder and change the name to Form1.cs

Keep the other generated project files in place, including `AnimatedWallpaper.csproj` and `Program.cs`.

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

### 5. Add your video

Place your own MP4 file in your Windows user's Videos folder (C:\Users\YOUR_NAME\Videos) and name it:

```text
wallpaper.mp4
```

The application locates this folder automatically, so you do not need to edit the source code to include your Windows username or a machine-specific file path.

The expected filename is `wallpaper.mp4`. If you want to use another filename, update the corresponding path in `Form1.cs`.

### 6. Build the application

Run:

```cmd
dotnet build
```

Wait for the build to complete. Resolve any compilation ERRORS before proceeding. A "succeeded with 1 warning(s)" is usually a A `WindowsBase` version conflict, this warning is distinct from a compilation error, and can be safely ignored. It is safe to assume that any "succeeded with n warning(s)" from this point forward can be ignored. 
 

### 7. Run the application

Run:

```cmd
dotnet run
```

The application checks that the video exists, initializes WebView2, loads the video, waits briefly for the browser's video surface to initialize, and then attempts desktop integration.

If everything works, the video should fill the desktop background while the desktop icons remain accessible. NOTE: The application will take up the entire screen to allow you to judge proper fitment. Simply clicking the screen will push the animated background to the back behind desktop shortcuts. 

If compilation fails, review the reported errors before attempting to run the application or reach out to me and I can push a hotfix/evaluate the error.


## Run Automatically on Windows Startup

Native Wallpaper can launch automatically when you sign in to Windows using Task Scheduler. This method was tested on Windows 11 25H2.

### 1. Publish the Application

Before publishing, close the running wallpaper application:
1. Open Task Manager by pressing `Ctrl + Shift + Esc`.
2. Find `AnimatedWallpaper` in the list of running apps or processes.
3. Right-click it and select **End task**. This closes the wallpaper application that is currently running.

In the same Command Prompt window you used earlier to run `dotnet run` (the one that is already open), type or copy and paste the following command, then press **Enter**:

```powershell
dotnet publish -c Release -r win-x64 --self-contained true
```

This creates a standalone Windows build in the publish output directory, typically:

`bin\Release\netX.X-windows\win-x64\publish\`

The exact path depends on the target .NET version in your project.

### 2. Move the Published Files to a Permanent Location

1. Open **File Explorer**.
2. Click the address bar at the top of the window, type or paste the following path, and press **Enter**:

   `%LOCALAPPDATA%\Programs`

   This opens the Programs folder inside your Windows user account's local AppData folder. Windows automatically resolves `%LOCALAPPDATA%` to the correct location for your account.

3. Inside the Programs folder, create a new folder named:

   `NativeWallpaper`

   Your permanent application folder will now be:

   `C:\Users\YOUR_NAME\%LOCALAPPDATA%\Programs\NativeWallpaper\`

4. Open your project's `publish` folder. It is typically located at:

   `C:\Users\YOUR_NAME\AnimatedWallpaper\bin\Release\netX.X-windows\win-x64\publish\`

5. Select **all files and subfolders** inside the `publish` folder and copy them.
6. Paste them into your new `NativeWallpaper` folder.
7. Confirm that `NativeWallpaper.exe` (or your project's actual executable name) is directly inside the permanent folder.
8. Double-click the executable to verify that the wallpaper works.

**Important:** Copy the contents of the `publish` folder, not the `publish` folder itself. Do not delete or move the permanent folder after configuring startup. Task Scheduler will launch the executable from this location.

### 3. Create a Task in Task Scheduler

1. Open the Windows Start menu, search for **Task Scheduler**, and launch it.
2. Select **Task Scheduler Library** in the left panel.
3. Click **Create Task** in the right panel.

**General tab**
- Name: `Native Wallpaper`
- Select **Run only when user is logged on**.
- Set **Configure for** to **Windows 10** if Windows 11 is not listed.

**Triggers tab**
1. Click **New**.
2. Set **Begin the task** to **At log on**.
3. Leave the delay disabled.
4. Click **OK**.

**Actions tab**
1. Click **New**.
2. Set **Action** to **Start a program**.
3. Click **Browse** and select the executable in your permanent application folder. Usually `C:\Users\YOUR_NAME \AppData\Local\Programs\NativeWallpaper\AnimatedWallpaper.exe`
4. In **Start in (optional)**, enter the full path to the folder containing the executable, without the executable filename. Usually `C:\Users\YOUR_NAME\AppData\Local\Programs\NativeWallpaper`
5. Click **OK**.

**Conditions tab**
- Uncheck **Start the task only if the computer is on AC power** so the wallpaper can start while running on battery.
- Review any other conditions that might prevent the task from running.

Click **OK** to save the task.

### 4. Test Automatic Startup

1. Return to **Task Scheduler Library**.
2. Find `Native Wallpaper`.
3. Right-click the task and select **Run**.
4. Confirm that the animated wallpaper appears.
5. Restart the computer and sign in to verify that it launches automatically.

### Notes

- The task runs when you sign in, not before the Windows desktop loads.
- Keep the application in its permanent folder. Moving or deleting the files will break the task.
- The wallpaper video must remain at the location expected by the application.
- This setup uses Task Scheduler rather than the Windows Startup folder.



## Additional Information/Troubleshooting Tips


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

Review the compiler errors. A warning does not necessarily indicate that the build failed. Resolve errors before running the application. Copilot and ChatGPT are useful tools to reference/ask questions. 

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

## Planned Future Improvements ( hopefully I remember this project :) )

- Support for multiple monitors.
- Automatic startup after user sign-in (DONE)
- Better recovery if the Windows desktop restarts.
- More robust detection of the correct desktop window (larger applicability window for different windows versions) 
- Configuration for video scaling and playback behavior.

## Windows Desktop Integration info (In case any of you are curious)

### The WorkerW approach

Windows desktop rendering involves shell-managed windows, including `Progman`, `WorkerW`, and `SHELLDLL_DefView`.

The implementation sends the `0x052C` message to `Progman` to request the creation of a desktop worker window. It then searches for a suitable target and uses `SetParent` to reparent the wallpaper window.

The code includes multiple discovery paths because desktop window arrangements can differ across Windows configurations.

It also changes the window styles to make the application a child window, applies layered-window attributes, and positions the window using `SetWindowPos`.

**Important:** The desktop integration is experimental. The relevant Windows API behavior is not guaranteed across all Windows 11 builds. If the video covers the screen, disappears, or attaches to the wrong window, terminate the application and review the window discovery logic.

## License

MIT License.

