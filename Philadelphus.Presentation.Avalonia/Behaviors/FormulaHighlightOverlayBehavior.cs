using System.Collections;
using System.Collections.Specialized;

using global::Avalonia;
using global::Avalonia.Controls;
using global::Avalonia.Controls.Documents;
using global::Avalonia.Media;

using Philadelphus.Presentation.Avalonia.Converters;
using Philadelphus.Presentation.ViewModels.ControlsVMs;

namespace Philadelphus.Presentation.Avalonia.Behaviors
{
    /// <summary>
    /// Раскрашивает текст формулы прямо в поле ввода: заполняет <see cref="TextBlock.Inlines" />
    /// цветными <see cref="Run" /> по сегментам подсветки. TextBlock размещается ПОД прозрачным
    /// (по тексту) TextBox строки формул — цвета видны «сквозь» него.
    /// Вес шрифта намеренно НЕ варьируется по виду сегмента: иначе ширины символов разойдутся
    /// с TextBox и переносы строк перестанут совпадать. Цвета — тема-зависимы
    /// (<see cref="FormulaHighlightKindToBrushConverter.GetBrush" />).
    /// </summary>
    public sealed class FormulaHighlightOverlayBehavior
    {
        private FormulaHighlightOverlayBehavior()
        {
        }

        /// <summary>Источник сегментов подсветки (обычно ObservableCollection из VM).</summary>
        public static readonly AttachedProperty<IEnumerable?> SegmentsProperty =
            AvaloniaProperty.RegisterAttached<FormulaHighlightOverlayBehavior, TextBlock, IEnumerable?>("Segments");

        public static IEnumerable? GetSegments(TextBlock o) => o.GetValue(SegmentsProperty);
        public static void SetSegments(TextBlock o, IEnumerable? value) => o.SetValue(SegmentsProperty, value);

        static FormulaHighlightOverlayBehavior()
        {
            SegmentsProperty.Changed.AddClassHandler<TextBlock>(OnSegmentsChanged);
        }

        private static void OnSegmentsChanged(TextBlock textBlock, AvaloniaPropertyChangedEventArgs e)
        {
            if (e.OldValue is INotifyCollectionChanged oldNotifier)
            {
                oldNotifier.CollectionChanged -= GetHandler(textBlock);
            }

            if (e.NewValue is INotifyCollectionChanged newNotifier)
            {
                newNotifier.CollectionChanged += GetHandler(textBlock);
            }

            Rebuild(textBlock);
        }

        // Один обработчик на TextBlock, чтобы корректно отписываться.
        private static readonly AttachedProperty<NotifyCollectionChangedEventHandler?> HandlerProperty =
            AvaloniaProperty.RegisterAttached<FormulaHighlightOverlayBehavior, TextBlock, NotifyCollectionChangedEventHandler?>("Handler");

        private static NotifyCollectionChangedEventHandler GetHandler(TextBlock textBlock)
        {
            var handler = textBlock.GetValue(HandlerProperty);
            if (handler is null)
            {
                handler = (_, _) => Rebuild(textBlock);
                textBlock.SetValue(HandlerProperty, handler);
            }

            return handler;
        }

        private static void Rebuild(TextBlock textBlock)
        {
            var inlines = textBlock.Inlines;
            if (inlines is null)
            {
                inlines = new InlineCollection();
                textBlock.Inlines = inlines;
            }

            inlines.Clear();

            if (GetSegments(textBlock) is not IEnumerable segments)
            {
                return;
            }

            foreach (var item in segments)
            {
                if (item is not FormulaHighlightSegmentVM segment)
                {
                    continue;
                }

                var run = new Run(segment.Text)
                {
                    Foreground = FormulaHighlightKindToBrushConverter.GetBrush(segment.Kind)
                };

                // Парная скобка под курсором: фон у инлайнов в Avalonia недоступен,
                // поэтому помечаем подчёркиванием (ширину символов не меняет — переносы не «едут»).
                if (segment.IsMatchingParenthesis)
                {
                    run.TextDecorations = TextDecorations.Underline;
                }

                inlines.Add(run);
            }
        }
    }
}
