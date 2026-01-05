using Philadelphus.Presentation.Wpf.UI.ViewModels;
using System.Collections;

namespace Philadelphus.Presentation.Wpf.UI.Models.Entities
{
    public class ParallelObservableCollectionVM<T> : ViewModelBase, IEnumerable
    {
        public List<T> Collection = new List<T>();
        public  void Add(T item)
        {
            Collection.Add(item);
            OnPropertyChanged(nameof(Collection));
            OnPropertyChanged();
        }

        public IEnumerator GetEnumerator()
        {
            return Collection.GetEnumerator();
        }

        public void Remove(T item)
        {
            Collection.Remove(item);
            OnPropertyChanged(nameof(Collection));
            OnPropertyChanged();
        }
    }
}
