
using System;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.WinForms;

namespace AnimatedWallpaper
{
    public partial class Form1 : Form
    {
        // CUSTOMIZE: Change the video filename here.
        private const string WallpaperFileName = "wallpaper.mp4";

        // CUSTOMIZE: Set true to show error dialogs.
        private const bool ShowErrorDialogs = true;

        private readonly WebView2 browser = new WebView2();

        private const int GWL_STYLE = -16;

        private const long WS_CHILD = 0x40000000L;
        private const long WS_POPUP = unchecked((long)0x80000000);

        private const uint SWP_NOACTIVATE = 0x0010;
        private const uint SWP_FRAMECHANGED = 0x0020;
        private const uint SWP_SHOWWINDOW = 0x0040;

        private const uint WM_SPAWN_WORKER = 0x052C;

        private static readonly IntPtr HWND_BOTTOM = new IntPtr(1);

        private bool attached;

        [DllImport("user32.dll", SetLastError = true,
            CharSet = CharSet.Unicode)]
        private static extern IntPtr FindWindow(
            string? className, string? windowName);

        [DllImport("user32.dll", SetLastError = true,
            CharSet = CharSet.Unicode)]
        private static extern IntPtr FindWindowEx(
            IntPtr parent, IntPtr childAfter,
            string? className, string? windowName);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr SendMessageTimeout(
            IntPtr hWnd, uint msg,
            IntPtr wParam, IntPtr lParam,
            uint flags, uint timeout,
            out IntPtr result);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr SetParent(
            IntPtr child, IntPtr newParent);

        [DllImport("user32.dll", EntryPoint = "GetWindowLongPtrW",
            SetLastError = true)]
        private static extern IntPtr GetWindowLongPtr64(
            IntPtr hWnd, int index);

        [DllImport("user32.dll", EntryPoint = "GetWindowLongW",
            SetLastError = true)]
        private static extern int GetWindowLong32(
            IntPtr hWnd, int index);

        [DllImport("user32.dll", EntryPoint = "SetWindowLongPtrW",
            SetLastError = true)]
        private static extern IntPtr SetWindowLongPtr64(
            IntPtr hWnd, int index, IntPtr newStyle);

        [DllImport("user32.dll", EntryPoint = "SetWindowLongW",
            SetLastError = true)]
        private static extern int SetWindowLong32(
            IntPtr hWnd, int index, int newStyle);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool SetWindowPos(
            IntPtr hWnd, IntPtr insertAfter,
            int x, int y, int width, int height,
            uint flags);

        private static IntPtr GetWindowLongPtr(
            IntPtr hWnd, int index)
        {
            return IntPtr.Size == 8
                ? GetWindowLongPtr64(hWnd, index)
                : new IntPtr(GetWindowLong32(hWnd, index));
        }

        private static IntPtr SetWindowLongPtr(
            IntPtr hWnd, int index, IntPtr newStyle)
        {
            return IntPtr.Size == 8
                ? SetWindowLongPtr64(hWnd, index, newStyle)
                : new IntPtr(SetWindowLong32(
                    hWnd, index, newStyle.ToInt32()));
        }

        public Form1()
        {
            Text = "Animated Wallpaper";
            FormBorderStyle = FormBorderStyle.None;
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.Manual;

            Bounds = Screen.PrimaryScreen!.Bounds;

            browser.Dock = DockStyle.Fill;
            Controls.Add(browser);

            Load += async (_, _) => await StartWallpaperAsync();
        }

        private async Task StartWallpaperAsync()
        {
            try
            {
                // Uses the current Windows user's Videos folder.
                string videoFolder = Environment.GetFolderPath(
                    Environment.SpecialFolder.MyVideos);

                string videoPath = Path.Combine(
                    videoFolder, WallpaperFileName);

                if (!File.Exists(videoPath))
                {
                    ShowError(
                        "Video not found:\n" + videoPath);

                    Close();
                    return;
                }

                await browser.EnsureCoreWebView2Async();

                browser.CoreWebView2.SetVirtualHostNameToFolderMapping(
                    "wallpaper.local",
                    videoFolder,
                    CoreWebView2HostResourceAccessKind.Allow);

                string html =
                    "<!DOCTYPE html>" +
                    "<html><head><meta charset='utf-8'>" +
                    "<style>" +
                    "html,body{margin:0;width:100%;height:100%;" +
                    "overflow:hidden;background:black;}" +
                    "video{width:100%;height:100%;" +
                    "object-fit:cover;display:block;}" +
                    "</style></head><body>" +
                    "<video autoplay muted loop playsinline>" +
                    "<source src='https://wallpaper.local/" +
                    WallpaperFileName +
                    "' type='video/mp4'>" +
                    "</video></body></html>";

                browser.NavigateToString(html);

                await Task.Delay(1200);

                if (!IsDisposed && IsHandleCreated)
                    PutBehindDesktop();
            }
            catch (Exception ex)
            {
                ShowError(ex.ToString());
                Close();
            }
        }

        private void PutBehindDesktop()
        {
            if (attached)
                return;

            IntPtr progman = FindWindow("Progman", null);

            if (progman == IntPtr.Zero)
            {
                ShowError("Could not find the Windows desktop.");
                return;
            }

            SendMessageTimeout(
                progman,
                WM_SPAWN_WORKER,
                new IntPtr(0xD),
                new IntPtr(1),
                0,
                1000,
                out _);

            IntPtr target = FindWindowEx(
                progman, IntPtr.Zero, "WorkerW", null);

            IntPtr desktopView = FindWindowEx(
                progman, IntPtr.Zero, "SHELLDLL_DefView", null);

            if (target == IntPtr.Zero)
            {
                IntPtr current = IntPtr.Zero;
                IntPtr iconHost = IntPtr.Zero;

                while ((current = FindWindowEx(
                    IntPtr.Zero, current, "WorkerW", null))
                    != IntPtr.Zero)
                {
                    IntPtr view = FindWindowEx(
                        current, IntPtr.Zero,
                        "SHELLDLL_DefView", null);

                    if (view != IntPtr.Zero)
                    {
                        iconHost = current;
                        desktopView = view;
                        break;
                    }
                }

                if (iconHost != IntPtr.Zero)
                {
                    target = FindWindowEx(
                        IntPtr.Zero, iconHost, "WorkerW", null);
                }
            }

            if (target == IntPtr.Zero)
                target = progman;

            IntPtr style = GetWindowLongPtr(Handle, GWL_STYLE);

            long newStyle =
                (style.ToInt64() & ~WS_POPUP) | WS_CHILD;

            SetWindowLongPtr(
                Handle, GWL_STYLE, new IntPtr(newStyle));

            SetLastError(0);

            IntPtr previousParent = SetParent(Handle, target);
            int parentError = Marshal.GetLastWin32Error();

            if (previousParent == IntPtr.Zero && parentError != 0)
            {
                ShowError(
                    "Could not attach to the desktop.\nError: " +
                    parentError);
                return;
            }

            Rectangle bounds = Screen.PrimaryScreen!.Bounds;

            IntPtr insertAfter = HWND_BOTTOM;

            if (target == progman && desktopView != IntPtr.Zero)
                insertAfter = desktopView;

            bool positioned = SetWindowPos(
                Handle,
                insertAfter,
                0, 0,
                bounds.Width, bounds.Height,
                SWP_NOACTIVATE |
                SWP_FRAMECHANGED |
                SWP_SHOWWINDOW);

            if (!positioned)
            {
                ShowError(
                    "Could not position the wallpaper.\nError: " +
                    Marshal.GetLastWin32Error());
                return;
            }

            attached = true;
        }

        private static void ShowError(string message)
        {
            if (ShowErrorDialogs)
            {
                MessageBox.Show(
                    message,
                    "Animated Wallpaper",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }
    }
}
