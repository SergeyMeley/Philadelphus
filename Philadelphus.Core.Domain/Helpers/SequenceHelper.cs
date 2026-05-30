namespace Philadelphus.Core.Domain.Helpers
{
    /// <summary>
    /// Помощник порядковых номеров
    /// </summary>
    public static class SequenceHelper
    {
        /// <summary>
        /// Кратность порядкового номера
        /// </summary>
        public static int Interval { get; } = 10;

        /// <summary>
        /// Получить следующий порядковый номер
        /// </summary>
        /// <param name="sequences">Занятые порядковые номера</param>
        /// <returns>Результат выполнения операции.</returns>
        public static int GetLastSequence(IEnumerable<int> sequences)
        {
            return (int)Math.Round(value: (double)sequences.Max() / 10, MidpointRounding.ToPositiveInfinity) * 10 + Interval;
        }

        /// <summary>
        /// Получить новый порядковый номер.
        /// </summary>
        /// <param name="sequences">Занятые порядковые номера.</param>
        /// <returns>Новый порядковый номер.</returns>
        public static long GetNewSequence(IEnumerable<long>? sequences = null)
        {
            return GetNewSequences(sequences).First();
        }

        /// <summary>
        /// Получить последовательность доступных кандидатов порядкового номера.
        /// </summary>
        /// <param name="sequences">Занятые порядковые номера.</param>
        /// <returns>Кандидаты порядкового номера.</returns>
        public static IEnumerable<long> GetNewSequences(IEnumerable<long>? sequences = null)
        {
            var existSequences = sequences?
                .Where(x => x > 0)
                .ToHashSet() ?? new HashSet<long>();

            var maxSequence = existSequences.Count == 0
                ? 0
                : existSequences.Max();

            var index = GetNextIntervalValue(maxSequence);
            while (index < long.MaxValue)
            {
                if (existSequences.Contains(index))
                {
                    index += Interval;
                    continue;
                }

                yield return index;
                index += Interval;
            }

            throw new InvalidOperationException("Не удалось подобрать свободный порядковый номер.");
        }

        private static long GetNextIntervalValue(long value)
        {
            return ((value / Interval) + 1) * Interval;
        }
    }
}
