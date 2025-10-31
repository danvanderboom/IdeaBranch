using System;
using System.ComponentModel;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Behaviors;

namespace IdeaBranch.App.Behaviors;

/// <summary>
/// Behavior that tracks text selection in an Editor and updates a ViewModel.
/// </summary>
public class EditorSelectionBehavior : Behavior<Editor>
{
    /// <summary>
    /// Gets or sets the binding context that has SelectionStart and SelectionLength properties.
    /// </summary>
    public object? SelectionContext { get; set; }

    protected override void OnAttachedTo(Editor bindable)
    {
        base.OnAttachedTo(bindable);
        bindable.TextChanged += OnTextChanged;
        // Note: Selection tracking requires platform-specific handlers
        // For now, this is a placeholder that can be extended with platform handlers
    }

    protected override void OnDetachingFrom(Editor bindable)
    {
        bindable.TextChanged -= OnTextChanged;
        base.OnDetachingFrom(bindable);
    }

    private void OnTextChanged(object? sender, TextChangedEventArgs e)
    {
        // Reset selection when text changes significantly
        // Platform-specific handlers should update SelectionStart/SelectionLength
        if (SelectionContext is INotifyPropertyChanged viewModel)
        {
            // Platform handler would set these properties
        }
    }
}

