using global::Avalonia;
using global::Avalonia.Controls;

namespace Philadelphus.Presentation.Avalonia.Views.Controls
{
    /// <summary>
    /// Редактор строки подключения SQLite.
    /// </summary>
    public partial class SqliteConnectionStringEditor : UserControl
    {
        public static readonly StyledProperty<bool> IsReadOnlyProperty =
            AvaloniaProperty.Register<SqliteConnectionStringEditor, bool>(nameof(IsReadOnly));

        public bool IsReadOnly
        {
            get => GetValue(IsReadOnlyProperty);
            set => SetValue(IsReadOnlyProperty, value);
        }

        public SqliteConnectionStringEditor()
        {
            InitializeComponent();
        }
    }
}
