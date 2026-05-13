using Philadelphus.Core.Domain.ExtensionSystem.Models;
using System.ComponentModel;

namespace Philadelphus.Core.Domain.ExtensionSystem.Infrastructure
{
    /// <summary>
    /// Задает контракт для работы с расширения.
    /// </summary>
    public interface IExtensionWidget : INotifyPropertyChanged
    {
        void SetExtension(IExtensionModel extension);
    }
}
