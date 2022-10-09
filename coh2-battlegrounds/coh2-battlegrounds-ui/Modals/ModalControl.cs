using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace Battlegrounds.UI.Modals;

/// <summary>
/// Enum defining the behaviour of the transparent background.
/// </summary>
public enum ModalBackgroundBehaviour {

    /// <summary>
    /// No behaviour is tied to the background.
    /// </summary>
    None,

    /// <summary>
    /// The modal will exit when the modal background mask is clicked.
    /// </summary>
    ExitWhenClicked,

    /// <summary>
    /// The modal will exit when the modal background bask is double-clicked.
    /// </summary>
    ExitWhenDoubleClicked,

}

/// <summary>
/// Provides a generic implementation for <see cref="UserControl"/> classes that require support for modals.
/// </summary>
public sealed class ModalControl : UserControl {

    /// <summary>
    /// Identifies the <see cref="Modal"/> attached property.
    /// </summary>
    public static readonly DependencyProperty ModalProperty = DependencyProperty.Register(nameof(Modal), typeof(Modal), typeof(ModalControl));

    /// <summary>
    /// Identifies the <see cref="IsModalActive"/> attached property.
    /// </summary>
    public static readonly DependencyProperty IsModalActiveProperty = DependencyProperty.Register(nameof(IsModalActive), typeof(bool), typeof(ModalControl));

    /// <summary>
    /// Identifies the <see cref="ModalMaskBehaviour"/> attached property.
    /// </summary>
    public static readonly DependencyProperty ModalMaskBehaviourProperty = DependencyProperty.Register(nameof(ModalMaskBehaviour), typeof(ModalBackgroundBehaviour), typeof(ModalControl));

    /// <summary>
    /// Identifies the <see cref="ModalMaskBehaviour"/> attached property.
    /// </summary>
    public static readonly DependencyProperty ModalMaskColourProperty = DependencyProperty.Register(nameof(ModalMaskColour), typeof(Brush), typeof(ModalControl));

    /// <summary>
    /// Identifies the <see cref="ModalMaskBehaviour"/> attached property.
    /// </summary>
    public static readonly DependencyProperty ModalMaskOpacityProperty = DependencyProperty.Register(nameof(ModalMaskOpacity), typeof(double), typeof(ModalControl));

    private Modal? m_currentModal;
    private object? m_backingContent;

    private readonly Grid m_contentCanvas;
    private readonly Rectangle m_contentRect;

    /// <summary>
    /// Get the currently active <see cref="Modals.Modal"/> instance.
    /// </summary>
    public Modal? Modal => this.m_currentModal;

    /// <summary>
    /// Get if there is currently a <see cref="Modals.Modal"/> being handled by the control.
    /// </summary>
    public bool IsModalActive => this.Modal is not null;

    /// <summary>
    /// Get or set the behaviour of the modal background mask.
    /// </summary>
    public ModalBackgroundBehaviour ModalMaskBehaviour {
        get => (ModalBackgroundBehaviour)this.GetValue(ModalMaskBehaviourProperty);
        set => this.SetValue(ModalMaskBehaviourProperty, value);
    }

    /// <summary>
    /// Get or set the colour of the modal background mask.
    /// </summary>
    public Brush? ModalMaskColour {
        get => this.GetValue(ModalMaskColourProperty) as Brush;
        set => this.SetValue(ModalMaskColourProperty, value);
    }

    /// <summary>
    /// Get or set the opacity of the modal background mask.
    /// </summary>
    public double ModalMaskOpacity {
        get => (double)this.GetValue(ModalMaskOpacityProperty);
        set => this.SetValue(ModalMaskOpacityProperty, value);
    }

    /// <summary>
    /// Initialise a new <see cref="ModalControl"/> instance.
    /// </summary>
    public ModalControl() : base() {
        this.m_currentModal = null;
        this.m_backingContent = null;
        this.m_contentCanvas = new();
        this.m_contentRect = new();
        this.m_contentRect.MouseDown += this.OnMaskMouseDown;
    }

    private void OnMaskMouseDown(object sender, MouseButtonEventArgs e) {

        // If no modal, do nothing (Safety check)
        if (!this.IsModalActive) {
            return;
        }

        // If either modal exit triggerer is set and happened, close the modal
        if ((e.ClickCount is 1 && this.ModalMaskBehaviour is ModalBackgroundBehaviour.ExitWhenClicked)
            || (e.ClickCount is >= 2 && this.ModalMaskBehaviour is ModalBackgroundBehaviour.ExitWhenDoubleClicked)) {
            this.Modal?.CloseModal();
        }

    }

    /// <summary>
    /// Show the view modal for specified <paramref name="modal"/> model.
    /// </summary>
    /// <param name="modal">The object to try and create a modal instance from.</param>
    /// <exception cref="Exception"></exception>
    /// <exception cref="ArgumentNullException"></exception>
    public void ShowModal(object modal) {

        // Check if null
        if (modal is null)
            throw new ArgumentNullException(nameof(modal));

        // Send call directly
        if (modal is Modal m) {
            this.ShowModal(m);
            return;
        }

        // Check if current application has a resource resolver
        if (Application.Current is not IResourceResolver resourceResolver) {
            throw new InvalidProgramException("Expected application to implement IResourceResolver!");
        }

        // Try find matching data template
        if (resourceResolver.TryFindDataTemplate(modal.GetType()) is not DataTemplate dataTemplate) {

            // Throw error => Failure
            throw new Exception("Failed to create modal view from view model.");

        }

        // Create content
        var content = dataTemplate.LoadContent();

        // Ensure valid
        if (content is Modal templatedModal) {
            templatedModal.DataContext = modal;
            this.ShowModal(templatedModal);
            if (modal is ModalBase mBase) {
                mBase.ShowModal(this);
            }
            return;
        }

    }

    /// <summary>
    /// Show the <paramref name="modal"/> using the modal procedure.
    /// </summary>
    /// <param name="modal">The <see cref="Modals.Modal"/> instance to display.</param>
    public void ShowModal(Modal modal) {

        // Close current modal
        if (this.m_currentModal is not null) {
            this.m_currentModal.CloseModal();
        }

        // Set current modal
        this.m_currentModal = modal;

        // Store the original content
        this.m_backingContent = this.Content;

        // Set dimensions of blocking content
        this.m_contentRect.Width = this.Width;
        this.m_contentRect.Height = this.Height;
        this.m_contentRect.Fill = this.ModalMaskColour;
        this.m_contentRect.Opacity = this.ModalMaskOpacity;

        // Set new content
        this.Content = this.m_contentCanvas;

        // Define new content to display
        this.m_contentCanvas.Children.Clear();
        this.m_contentCanvas.Children.Add(this.m_backingContent as UIElement);
        this.m_contentCanvas.Children.Add(this.m_contentRect);
        this.m_contentCanvas.Children.Add(this.m_currentModal);

        // Inform the modal it's now showing
        this.m_currentModal.DisplayModal(this);

    }

    /// <summary>
    /// Close the current modal effect.
    /// </summary>
    public void CloseModal() {

        // Make sure there's a modal to close
        if (!this.IsModalActive) {
            return;
        }

        // Inform the modal base it's being closed
        if (this.m_currentModal is Modal m && m.DataContext is ModalBase mb)
            mb.CloseModal();

        // Remove reference to current modal
        this.m_currentModal = null;

        // Update content
        this.m_contentCanvas.Children.Clear();
        this.Content = this.m_backingContent;

        // Remove reference to backing content
        this.m_backingContent = null;

    }

}
