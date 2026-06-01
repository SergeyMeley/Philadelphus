using Philadelphus.Core.Domain.Entities.Enums;

namespace Philadelphus.Core.Domain.FormulaEngine.Contracts
{
    /// <summary>
    /// Описание одного аргумента формулы.
    /// </summary>
    public sealed class FormulaArgumentDefinition
    {
        /// <summary>
        /// Имя аргумента в сигнатуре формулы.
        /// </summary>
        public required string Name { get; init; }

        /// <summary>
        /// Человекочитаемое описание аргумента.
        /// </summary>
        public string? Description { get; init; }

        /// <summary>
        /// Ожидаемый системный тип значения, если аргумент типизирован.
        /// </summary>
        public SystemBaseType? ExpectedType { get; init; }

        /// <summary>
        /// Признак обязательности аргумента.
        /// </summary>
        public bool IsRequired { get; init; } = true;

        /// <summary>
        /// Значение по умолчанию для необязательного аргумента.
        /// </summary>
        public object? DefaultValue { get; init; }
    }
}
