using global::Avalonia;
using global::Avalonia.Controls;

namespace Philadelphus.Presentation.Avalonia.Views.Controls.DetailedInformationGrids
{
    /// <summary>
    /// Логика взаимодействия для DataStorageDetails.axaml
    /// </summary>
    public partial class DataStorageDetails : UserControl
    {
        //TODO: Мб удалить
        public static readonly StyledProperty<bool> IsReadOnlyProperty =
            AvaloniaProperty.Register<DataStorageDetails, bool>(nameof(IsReadOnly));

        public bool IsReadOnly
        {
            get { return GetValue(IsReadOnlyProperty); }
            set { SetValue(IsReadOnlyProperty, value); }
        }

        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="DataStorageDetails" />.
        /// </summary>
        public DataStorageDetails()
        {
            InitializeComponent();
        }
    }
}
