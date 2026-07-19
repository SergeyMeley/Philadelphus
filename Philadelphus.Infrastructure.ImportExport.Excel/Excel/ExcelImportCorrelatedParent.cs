namespace Philadelphus.Infrastructure.ImportExport.Excel;

/// <summary>
/// Сопоставляет значение родительского ключа с временной корреляцией и именем
/// листа, которое пока необходимо совместимому FK-атрибуту.
/// </summary>
/// <param name="CorrelationId">Временный идентификатор родительской строки.</param>
/// <param name="LeafName">Имя материализуемого родительского листа.</param>
internal sealed record ExcelImportCorrelatedParent(Guid CorrelationId, string LeafName);
