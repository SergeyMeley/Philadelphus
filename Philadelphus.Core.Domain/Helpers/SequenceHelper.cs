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
        /// <returns></returns>
        public static int GetLastSequence(IEnumerable<int> sequences)
        {
            return (int)Math.Round(value: (double)sequences.Max() / 10, MidpointRounding.ToPositiveInfinity) * 10 + Interval;
        }
    }
}
