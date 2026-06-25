using System;
using System.ComponentModel;
using System.Linq;

using global::Avalonia.Controls;
using global::Avalonia.Threading;

namespace Philadelphus.Presentation.Avalonia.Views.Controls.RepositoryExplorer
{
    /// <summary>
    /// Обозреватель репозитория Чубушника.
    /// </summary>
    public partial class RepositoryExplorer : UserControl
    {
        private INotifyPropertyChanged? _viewModel;

        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="RepositoryExplorer" />.
        /// </summary>
        public RepositoryExplorer()
        {
            InitializeComponent();
            DataContextChanged += OnDataContextChanged;
        }

        private void OnDataContextChanged(object? sender, EventArgs e)
        {
            if (_viewModel != null)
            {
                _viewModel.PropertyChanged -= OnViewModelPropertyChanged;
            }

            _viewModel = DataContext as INotifyPropertyChanged;

            if (_viewModel != null)
            {
                _viewModel.PropertyChanged += OnViewModelPropertyChanged;
            }

            EnsureVisibleTabSelected();
        }

        private void OnViewModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            // Видимость вкладок «Значение»/«Наследники» зависит от выбранного элемента.
            // После их пересчета выбор мог остаться на ставшей невидимой вкладке.
            if (e.PropertyName is "IsSystemBaseLeaveControlVisible"
                or "IsParentControlVisible"
                or "SelectedRepositoryMember")
            {
                // Откладываем: к этому моменту привязки IsVisible на вкладках уже обновятся.
                Dispatcher.UIThread.Post(EnsureVisibleTabSelected);
            }
        }

        /// <summary>
        /// Если выбранная вкладка скрыта (например «Значение» для не-листа), переключает на первую видимую.
        /// Avalonia, в отличие от WPF, не уводит выбор с свернутой/скрытой вкладки автоматически.
        /// </summary>
        private void EnsureVisibleTabSelected()
        {
            if (CurrentElementTabs.SelectedItem is TabItem { IsVisible: true })
            {
                return;
            }

            var firstVisible = CurrentElementTabs.Items.OfType<TabItem>().FirstOrDefault(tab => tab.IsVisible);
            if (firstVisible != null)
            {
                CurrentElementTabs.SelectedItem = firstVisible;
            }
        }
    }
}
