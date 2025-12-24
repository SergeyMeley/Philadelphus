using Philadelphus.Core.Domain.ExtensionSystem.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Philadelphus.Core.Domain.ExtensionSystem.Infrastructure
{
    public interface IExtensionWidget : INotifyPropertyChanged
    {
        void SetExtension(IExtensionModel extension);
    }
}
