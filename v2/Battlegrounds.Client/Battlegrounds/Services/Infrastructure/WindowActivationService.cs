using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;

using Microsoft.Extensions.Logging;

namespace Battlegrounds.Services.Infrastructure;

/// <summary>
/// Activates the main window through WPF, falling back to flashing its taskbar button.
/// </summary>
public sealed class WindowActivationService(ILogger<WindowActivationService> logger) : IWindowActivationService {

    private const uint FlashAll = 0x00000003;

    private const uint FlashUntilForeground = 0x0000000C;

    [StructLayout(LayoutKind.Sequential)]
    private struct FlashWindowInfo {
        public uint cbSize;
        public IntPtr hwnd;
        public uint dwFlags;
        public uint uCount;
        public uint dwTimeout;
    }

    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool FlashWindowEx(ref FlashWindowInfo info);

    private readonly ILogger<WindowActivationService> _logger = logger;

    public void Activate() {

        Application? application = Application.Current;
        if (application is null) {
            return;
        }

        application.Dispatcher.Invoke(() => {

            if (application.MainWindow is not { } window) {
                _logger.LogDebug("There is no main window to activate yet.");
                return;
            }

            try {

                if (window.WindowState is WindowState.Minimized) {
                    window.WindowState = WindowState.Normal;
                }

                if (window.Activate()) {
                    return;
                }

                Flash(window);

            } catch (Exception ex) {
                _logger.LogDebug(ex, "The main window could not be brought forward.");
            }

        });

    }

    private void Flash(Window window) {

        IntPtr handle = new WindowInteropHelper(window).Handle;
        if (handle == IntPtr.Zero) {
            return;
        }

        FlashWindowInfo info = new() {
            cbSize = (uint)Marshal.SizeOf<FlashWindowInfo>(),
            hwnd = handle,
            dwFlags = FlashAll | FlashUntilForeground,
            uCount = uint.MaxValue,
            dwTimeout = 0,
        };

        FlashWindowEx(ref info);

    }

}
