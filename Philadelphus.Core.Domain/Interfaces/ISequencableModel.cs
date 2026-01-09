namespace Philadelphus.Core.Domain.Interfaces
{
    /// <summary>
    /// Упорядоченный
    /// </summary>
    public interface ISequencableModel
    {
        /// <summary>
        /// Порядковый номер
        /// </summary>
        public long Sequence { get; set; }
    }
}
