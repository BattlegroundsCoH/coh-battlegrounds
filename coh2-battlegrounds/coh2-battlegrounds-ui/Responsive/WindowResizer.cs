using System;
using System.Runtime.InteropServices;
using System.Windows.Interop;
using System.Windows;
using System.Windows.Media;

namespace Battlegrounds.UI.Responsive;

/// <summary>
/// The dock position of the window
/// </summary>
public enum WindowDockPosition {
    /// <summary>
    /// Not docked
    /// </summary>
    Undocked,
    /// <summary>
    /// Docked to the left of the screen
    /// </summary>
    Left,
    /// <summary>
    /// Docked to the right of the screen
    /// </summary>
    Right,
}

/// <summary>
/// Fixes the issue with Windows of Style <see cref="WindowStyle.None"/> covering the taskbar
/// </summary>
public sealed class WindowResizer {
    #region Private Members

    /// <summary>
    /// The window to handle the resizing for
    /// </summary>
    private readonly Window m_window;

    /// <summary>
    /// The last calculated available screen size
    /// </summary>
    private Rect m_ScreenSize = new Rect();

    /// <summary>
    /// How close to the edge the window has to be to be detected as at the edge of the screen
    /// </summary>
    private readonly int m_edgeTolerance = 2;

    /// <summary>
    /// The transform matrix used to convert WPF sizes to screen pixels
    /// </summary>
    private Matrix m_transformToDevice;

    /// <summary>
    /// The last screen the window was on
    /// </summary>
    private IntPtr m_lastScreen;

    /// <summary>
    /// The last known dock position
    /// </summary>
    private WindowDockPosition m_lastDock = WindowDockPosition.Undocked;

    #endregion

    #region Dll Imports

    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    static extern bool GetCursorPos(out POINT lpPoint);

    [DllImport("user32.dll")]
    static extern bool GetMonitorInfo(IntPtr hMonitor, MONITORINFO lpmi);

    [DllImport("user32.dll", SetLastError = true)]
    static extern IntPtr MonitorFromPoint(POINT pt, MonitorOptions dwFlags);

    #endregion

    #region Public Events

    /// <summary>
    /// Called when the window dock position changes
    /// </summary>
    public event Action<WindowDockPosition> WindowDockChanged = (dock) => { };

    #endregion

    #region Constructor

    /// <summary>
    /// Default constructor
    /// </summary>
    /// <param name="window">The window to monitor and correctly maximize</param>
    /// <param name="adjustSize">The callback for the host to adjust the maximum available size if needed</param>
    public WindowResizer(Window window) {
        
        // Set target window
        this.m_window = window;

        // Create transform visual (for converting WPF size to pixel size)
        this.GetTransform();

        // Listen out for source initialized to setup
        this.m_window.SourceInitialized += Window_SourceInitialized;

        // Monitor for edge docking
        this.m_window.SizeChanged += Window_SizeChanged;

    }

    #endregion

    #region Initialize

    /// <summary>
    /// Gets the transform object used to convert WPF sizes to screen pixels
    /// </summary>
    private void GetTransform() {

        // Get the visual source
        var source = PresentationSource.FromVisual(this.m_window);

        // Reset the transform to default
        this.m_transformToDevice = default;

        // If we cannot get the source, ignore
        if (source == null)
            return;

        // Otherwise, get the new transform object
        this.m_transformToDevice = source.CompositionTarget.TransformToDevice;
    
    }

    /// <summary>
    /// Initialize and hook into the windows message pump
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void Window_SourceInitialized(object? sender, EventArgs e) {

        // Get the handle of this window
        var handle = (new WindowInteropHelper(this.m_window)).Handle;
        var handleSource = HwndSource.FromHwnd(handle);

        // If not found, end
        if (handleSource == null)
            return;

        // Hook into Windows messages
        handleSource.AddHook(WindowProc);

    }

    #endregion

    #region Edge Docking

    /// <summary>
    /// Monitors for size changes and detects if the window has been docked (Aero snap) to an edge
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void Window_SizeChanged(object sender, SizeChangedEventArgs e) {

        // We cannot find positioning until the window transform has been established
        if (this.m_transformToDevice == default)
            return;

        // Get the WPF size
        var size = e.NewSize;

        // Get window rectangle
        var top = this.m_window.Top;
        var left = this.m_window.Left;
        var bottom = top + size.Height;
        var right = left + this.m_window.Width;

        // Get window position/size in device pixels
        var windowTopLeft = this.m_transformToDevice.Transform(new Point(left, top));
        var windowBottomRight = this.m_transformToDevice.Transform(new Point(right, bottom));

        // Check for edges docked
        var edgedTop = windowTopLeft.Y <= (this.m_ScreenSize.Top + this.m_edgeTolerance);
        var edgedLeft = windowTopLeft.X <= (this.m_ScreenSize.Left + this.m_edgeTolerance);
        var edgedBottom = windowBottomRight.Y >= (this.m_ScreenSize.Bottom - this.m_edgeTolerance);
        var edgedRight = windowBottomRight.X >= (this.m_ScreenSize.Right - this.m_edgeTolerance);

        // Get docked position
        var dock = WindowDockPosition.Undocked;

        // Left docking
        if (edgedTop && edgedBottom && edgedLeft)
            dock = WindowDockPosition.Left;
        else if (edgedTop && edgedBottom && edgedRight)
            dock = WindowDockPosition.Right;
        // None
        else
            dock = WindowDockPosition.Undocked;

        // If dock has changed
        if (dock != this.m_lastDock)
            // Inform listeners
            WindowDockChanged(dock);

        // Save last dock position
        this.m_lastDock = dock;

    }

    #endregion

    #region Windows Proc

    /// <summary>
    /// Listens out for all windows messages for this window
    /// </summary>
    /// <param name="hwnd"></param>
    /// <param name="msg"></param>
    /// <param name="wParam"></param>
    /// <param name="lParam"></param>
    /// <param name="handled"></param>
    /// <returns></returns>
    private IntPtr WindowProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled) {
        switch (msg) {
            // Handle the GetMinMaxInfo of the Window
            case 0x0024:/* WM_GETMINMAXINFO */
                WmGetMinMaxInfo(hwnd, lParam);
                handled = true;
                break;
        }

        return (IntPtr)0;
    }

    #endregion

    /// <summary>
    /// Get the min/max window size for this window
    /// Correctly accounting for the taskbar size and position
    /// </summary>
    /// <param name="_">Window handle (Not Used)</param>
    /// <param name="lParam"></param>
    private void WmGetMinMaxInfo(IntPtr _, IntPtr lParam) {

        // Get the point position to determine what screen we are on
        GetCursorPos(out POINT lMousePosition);

        // Get the primary monitor at cursor position 0,0
        var lPrimaryScreen = MonitorFromPoint(new POINT(0, 0), MonitorOptions.MONITOR_DEFAULTTOPRIMARY);

        // Try and get the primary screen information
        var lPrimaryScreenInfo = new MONITORINFO();
        if (GetMonitorInfo(lPrimaryScreen, lPrimaryScreenInfo) == false)
            return;

        // Now get the current screen
        var lCurrentScreen = MonitorFromPoint(lMousePosition, MonitorOptions.MONITOR_DEFAULTTONEAREST);

        // If this has changed from the last one, update the transform
        if (lCurrentScreen != this.m_lastScreen || this.m_transformToDevice == default)
            this.GetTransform();

        // Store last know screen
        this.m_lastScreen = lCurrentScreen;

        // Get min/max structure to fill with information
        if (Marshal.PtrToStructure(lParam, typeof(MINMAXINFO)) is not MINMAXINFO lMmi) {
            return;
        }

        // If it is the primary screen, use the rcWork variable
        if (lPrimaryScreen.Equals(lCurrentScreen) == true) {
            lMmi.ptMaxPosition.X = lPrimaryScreenInfo.rcWork.Left;
            lMmi.ptMaxPosition.Y = lPrimaryScreenInfo.rcWork.Top;
            lMmi.ptMaxSize.X = lPrimaryScreenInfo.rcWork.Right - lPrimaryScreenInfo.rcWork.Left;
            lMmi.ptMaxSize.Y = lPrimaryScreenInfo.rcWork.Bottom - lPrimaryScreenInfo.rcWork.Top;
        }
        // Otherwise it's the rcMonitor values
        else {
            lMmi.ptMaxPosition.X = lPrimaryScreenInfo.rcMonitor.Left;
            lMmi.ptMaxPosition.Y = lPrimaryScreenInfo.rcMonitor.Top;
            lMmi.ptMaxSize.X = lPrimaryScreenInfo.rcMonitor.Right - lPrimaryScreenInfo.rcMonitor.Left;
            lMmi.ptMaxSize.Y = lPrimaryScreenInfo.rcMonitor.Bottom - lPrimaryScreenInfo.rcMonitor.Top;
        }

        // Set min size
        var minSize = m_transformToDevice.Transform(new Point(this.m_window.MinWidth, this.m_window.MinHeight));

        lMmi.ptMinTrackSize.X = (int)minSize.X;
        lMmi.ptMinTrackSize.Y = (int)minSize.Y;

        // Store new size
        this.m_ScreenSize = new Rect(lMmi.ptMaxPosition.X, lMmi.ptMaxPosition.Y, lMmi.ptMaxSize.X, lMmi.ptMaxSize.Y);

        // Now we have the max size, allow the host to tweak as needed
        Marshal.StructureToPtr(lMmi, lParam, true);
    }

}

#region Dll Helper Structures

    enum MonitorOptions : uint {
        MONITOR_DEFAULTTONULL = 0x00000000,
        MONITOR_DEFAULTTOPRIMARY = 0x00000001,
        MONITOR_DEFAULTTONEAREST = 0x00000002
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
    public class MONITORINFO {
        public int cbSize = Marshal.SizeOf(typeof(MONITORINFO));
        public Rectangle rcMonitor = new Rectangle();
        public Rectangle rcWork = new Rectangle();
        public int dwFlags = 0;
    }


    [StructLayout(LayoutKind.Sequential)]
    public struct Rectangle {
        public int Left, Top, Right, Bottom;

        public Rectangle(int left, int top, int right, int bottom) {
            this.Left = left;
            this.Top = top;
            this.Right = right;
            this.Bottom = bottom;
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct MINMAXINFO {
        public POINT ptReserved;
        public POINT ptMaxSize;
        public POINT ptMaxPosition;
        public POINT ptMinTrackSize;
        public POINT ptMaxTrackSize;
    };

    [StructLayout(LayoutKind.Sequential)]
    public struct POINT {
        /// <summary>
        /// x coordinate of point.
        /// </summary>
        public int X;
        /// <summary>
        /// y coordinate of point.
        /// </summary>
        public int Y;

        /// <summary>
        /// Construct a point of coordinates (x,y).
        /// </summary>
        public POINT(int x, int y) {
            this.X = x;
            this.Y = y;
        }
    }

#endregion
