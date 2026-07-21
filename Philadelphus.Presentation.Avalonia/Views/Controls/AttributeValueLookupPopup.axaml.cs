using global::Avalonia.Controls.Primitives;
using global::Avalonia.Interactivity;

namespace Philadelphus.Presentation.Avalonia.Views.Controls;

/// <summary>
/// Popup расширенного поиска значения одиночного пользовательского атрибута.
/// </summary>
public partial class AttributeValueLookupPopup : Popup
{
    /// <summary>
    /// Инициализирует popup расширенного поиска.
    /// </summary>
    public AttributeValueLookupPopup()
    {
        InitializeComponent();
    }

    private void ClosePopup(object? sender, RoutedEventArgs eventArgs) =>
        IsOpen = false;
}
