using BattlegroundsApp.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace BattlegroundsApp;

public class MainWindowViewModel : ViewModelBase {

    public string Title { get; } = "Company of Heroes 2: Battlegrounds Mod Launcher";

    /// <summary>
    /// The window that this view model controls
    /// </summary>
    private Window m_window;

    /// <summary>
    /// The margin around the window to allow drop shadow
    /// </summary>
    private int m_outerMarginSize = 10;

    /// <summary>
    /// The radius of the edges of the window
    /// </summary>
    private int m_windowRadius = 10;

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
    public int TitleHeight { get; set; } = 30;

    /// <summary>
    /// Default Constrictor
    /// </summary>
    /// <param name="window"></param>
    public MainWindowViewModel(Window window) {

        this.m_window = window;

        // Listen for the window resizing
        this.m_window.StateChanged += (sender, e) => {

            OnPropertyChanged(nameof(ResizeBorderThickness));
            OnPropertyChanged(nameof(OuterMarginSize));
            OnPropertyChanged(nameof(OuterMarginThickness));
            OnPropertyChanged(nameof(WindowRadius));
            OnPropertyChanged(nameof(WindowCornerRadius));

        };

    }

}
