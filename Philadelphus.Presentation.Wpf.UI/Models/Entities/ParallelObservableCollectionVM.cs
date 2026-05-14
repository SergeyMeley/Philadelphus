using Philadelphus.Presentation.Wpf.UI.ViewModels;
using System.Collections;

namespace Philadelphus.Presentation.Wpf.UI.Models.Entities
{
    /// <summary>
    /// Модель представления для ParallelObservableCollectionVM.
    /// </summary>
    public class ParallelObservableCollectionVM<T> : ViewModelBase, IEnumerable
    {
        public List<T> Collection = new List<T>();

        /// <summary>
        /// Добавляет данные Add.
        /// </summary>
        /// <param name="item">Элемент.</param>
        public  void Add(T item)
        {
            Collection.Add(item);
            OnPropertyChanged(nameof(Collection));
            OnPropertyChanged();
        }

        /// <summary>
        /// Получает данные GetEnumerator.
        /// </summary>
        /// <returns>Полученные данные.</returns>
        public IEnumerator GetEnumerator()
        {
            return Collection.GetEnumerator();
        }

        /// <summary>
        /// Удаляет данные Remove.
        /// </summary>
        /// <param name="item">Элемент.</param>
        public void Remove(T item)
        {
            Collection.Remove(item);
            OnPropertyChanged(nameof(Collection));
            OnPropertyChanged();
        }
    }
}
