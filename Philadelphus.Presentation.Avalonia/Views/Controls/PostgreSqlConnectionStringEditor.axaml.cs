using global::Avalonia;
using global::Avalonia.Controls;

namespace Philadelphus.Presentation.Avalonia.Views.Controls
{
    /// <summary>
    /// Редактор строки подключения PostgreSQL.
    /// </summary>
    public partial class PostgreSqlConnectionStringEditor : UserControl
    {
        public static readonly StyledProperty<bool> IsReadOnlyProperty =
            AvaloniaProperty.Register<PostgreSqlConnectionStringEditor, bool>(nameof(IsReadOnly));

        public bool IsReadOnly
        {
            get => GetValue(IsReadOnlyProperty);
            set => SetValue(IsReadOnlyProperty, value);
        }

        public PostgreSqlConnectionStringEditor() { InitializeComponent(); }
    }
}
