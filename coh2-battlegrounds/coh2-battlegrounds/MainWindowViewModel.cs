using Battlegrounds.UI;
using Battlegrounds.UI.Responsive;

using System.Diagnostics;
using System.Reflection;
using System.Windows;
using System.Windows.Input;

namespace BattlegroundsApp;

public class MainWindowViewModel : ViewModelBase {

    /// <summary>
    /// The window that this view model controls
    /// </summary>
    private readonly Window m_window;

    /// <summary>
    /// The margin around the window to allow drop shadow
    /// </summary>
    private int m_outerMarginSize = 10;

    /// <summary>
    /// The radius of the edges of the window
    /// </summary>
    private int m_windowRadius = 10;

    /// <summary>
    /// The application version
    /// </summary>
    private readonly string? m_version;

    /// <summary>
    /// The window title
    /// </summary>
    public string Title { get; set; }

    /// <summary>
    /// Minimum width of the window
    /// </summary>
    public double WindowMinimumWidth { get; set; } = 1400.0;

    /// <summary>
    /// Minimum height of the window
    /// </summary>
    public double WindowMinimumHeight { get; set; } = 850.0;

    /// <summary>
    /// The size of the resize border around the window
    /// </summary>
    public int ResizeBorder { get; set; } = 1;

    /// <summary>
    /// The size of the resize border around the window, with the outer margin
    /// </summary>
    public Thickness ResizeBorderThickness { get { return new(ResizeBorder + OuterMarginSize); } }

    /// <summary>
    /// The margin around the window to allow drop shadow
    /// </summary>
    public int OuterMarginSize { 
    
        get {
            return m_window.WindowState == WindowState.Maximized ? 0 : m_outerMarginSize; 
        }

        set {
            m_outerMarginSize = value;
        }

    }

    public Thickness OuterMarginThickness { get { return new(OuterMarginSize); } }

    /// <summary>
    /// The radius of the edges of the window
    /// </summary>
    public int WindowRadius { 
    
        get {
            return m_window.WindowState == WindowState.Maximized ? 0 : m_windowRadius;
        }

        set {
            m_windowRadius = value;
        }

    }

    /// <summary>
    /// The radius of the edges of the window
    /// </summary>
    public CornerRadius WindowCornerRadius { get { return new(WindowRadius); } }

    /// <summary>
    /// The height of the title bar / the caption
    /// </summary>
    public int TitleHeight { get; set; } = 32;

    public GridLength TitleHeightGridLength { get { return new(TitleHeight + ResizeBorder); } }

    /// <summary>
    /// The command to minimize the window
    /// </summary>
    public ICommand MinimizeCommand { get; set; }

    /// <summary>
    /// The command to maximize the window
    /// </summary>
    public ICommand MaximizeCommand { get; set; }

    /// <summary>
    /// The command to close the window
    /// </summary>
    public ICommand CloseCommand { get; set; }

    /// <summary>
    /// The command to show the system menu
    /// </summary>
    public ICommand MenuCommand { get; set; }

    public override bool SingleInstanceOnly => true;

    public override bool KeepAlive => true;

    /// <summary>
    /// Default Constrictor
    /// </summary>
    /// <param name="window"></param>
    public MainWindowViewModel(Window window) {

        // Get version
        Assembly assembly = Assembly.GetExecutingAssembly();
        FileVersionInfo fvi = FileVersionInfo.GetVersionInfo(assembly.Location);
        this.m_version = fvi.FileVersion;

        this.Title = $"Company of Heroes 2: Battlegrounds Mod Launcher | v{this.m_version}";

        this.m_window = window;

        // Listen for the window resizing
        this.m_window.StateChanged += (sender, e) => {

            this.Notify(nameof(ResizeBorderThickness));
            this.Notify(nameof(OuterMarginSize));
            this.Notify(nameof(OuterMarginThickness));
            this.Notify(nameof(WindowRadius));
            this.Notify(nameof(WindowCornerRadius));

        };

        // Create commands
        this.MinimizeCommand = new RelayCommand(() => this.m_window.WindowState = WindowState.Minimized);
        this.MaximizeCommand = new RelayCommand(() => this.m_window.WindowState ^= WindowState.Maximized);
        // TODO : Move closing logic from View here
        this.CloseCommand = new RelayCommand(() => this.m_window.Close());
        this.MenuCommand = new RelayCommand(() => SystemCommands.ShowSystemMenu(this.m_window, GetMousePosition()));

        // Fix window resize issue
        var resizer = new WindowResizer(this.m_window);

    }

    /// <summary>
    /// Gets the current mouse position on the screen
    /// </summary>
    /// <returns></returns>
    private Point GetMousePosition() {

        // Get position of the mouse relative to the window
        var position = Mouse.GetPosition(this.m_window);

        // Add the window position so its to screen
        return new Point(position.X + this.m_window.Left, position.Y + this.m_window.Top);

    }

}
