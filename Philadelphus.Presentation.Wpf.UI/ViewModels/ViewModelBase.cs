using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Philadelphus.Presentation.Wpf.UI.ViewModels
{
    public abstract class ViewModelBase : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        protected internal virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        protected internal virtual void OnPropertyChangedRecursive(HashSet<object> visited = null)
        {
            visited ??= new HashSet<object>();

            if (!visited.Add(this))
                return;

            OnPropertyChanged(null);

            foreach (var property in GetType().GetProperties())
            {
                // пропускаем индексаторы
                if (property.GetIndexParameters().Length > 0)
                    continue;

                var value = property.GetValue(this);

                if (value == null)
                    continue;

                // одиночная ViewModel
                if (value is ViewModelBase vm)
                {
                    vm.OnPropertyChangedRecursive(visited);
                }
                // коллекция
                else if (value is System.Collections.IEnumerable enumerable)
                {
                    foreach (var item in enumerable)
                    {
                        if (item is ViewModelBase childVm)
                        {
                            childVm.OnPropertyChangedRecursive(visited);
                        }
                    }
                }
            }
        }
        protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
        {
            if (Equals(field, value)) return false;
            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }
    }
}
