namespace Philadelphus.Presentation.Services.Interfaces;

/// <summary>
/// Сообщает оконному сервису, что связанное с моделью представления окно нужно закрыть.
/// </summary>
public interface IWindowCloseRequestSource
{
    /// <summary>
    /// Возникает при необходимости закрыть связанное окно.
    /// </summary>
    event EventHandler? CloseRequested;
}
