using Philadelphus.Core.Domain.ExtensionSystem.Models;
using System.ComponentModel;

namespace Philadelphus.Core.Domain.ExtensionSystem.Infrastructure
{
    public interface IExtensionWidget : INotifyPropertyChanged
    {
        void SetExtension(IExtensionModel extension);
    }
}
