using Philadelphus.WpfApplication.ViewModels;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Philadelphus.WpfApplication.Models.Entities
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
