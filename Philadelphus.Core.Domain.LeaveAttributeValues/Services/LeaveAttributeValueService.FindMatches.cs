using Philadelphus.Core.Domain.Contracts.LeaveAttributeValues;
using Philadelphus.Core.Domain.Entities.MainEntities.PhiladelphusRepositoryMembers.ShrubMembers.WorkingTreeMembers;
using Philadelphus.Core.Domain.Entities.MainEntityContent.Attributes;
using Philadelphus.Core.Domain.LeaveAttributeValues.Signatures;

namespace Philadelphus.Core.Domain.LeaveAttributeValues.Services;

/// <summary>
/// Выполняет переиспользуемые операции поиска, заполнения и создания листов
/// по значениям атрибутов.
/// </summary>
public sealed partial class LeaveAttributeValueService
{
    /// <summary>
    /// Находит кандидатов, совпадающих по значениям явно переданных атрибутов.
    /// </summary>
    /// <param name="expectedAttributes">Ожидаемые атрибуты.</param>
    /// <param name="candidates">Кандидаты для поиска.</param>
    /// <returns>Результат поиска или невалидный результат при ошибке значения.</returns>
    public LeaveAttributeMatchResult FindMatches(
        IEnumerable<ElementAttributeModel> expectedAttributes,
        IEnumerable<TreeLeaveModel> candidates)
    {
        ArgumentNullException.ThrowIfNull(expectedAttributes);
        ArgumentNullException.ThrowIfNull(candidates);

        var expectedSignature = LeaveAttributeValueSignature.Create(expectedAttributes);
        if (expectedSignature.IsValid == false)
            return new(LeaveAttributeMatchStatus.Invalid, []);

        var declaringUuids = expectedSignature.DeclaringUuids.ToHashSet();
        var matches = new List<TreeLeaveModel>();

        foreach (var candidate in candidates)
        {
            ArgumentNullException.ThrowIfNull(candidate);

            // Поиск может использовать подмножество атрибутов, поэтому сигнатура
            // кандидата строится только по переданным объявлениям.
            var candidateAttributes = candidate.Attributes
                .Where(x => declaringUuids.Contains(x.DeclaringUuid));
            var candidateSignature = LeaveAttributeValueSignature.Create(candidateAttributes);

            // Невалидный кандидат нельзя безопасно отбросить: после исправления
            // ссылки или формулы он может оказаться дополнительным совпадением.
            if (candidateSignature.IsValid == false)
                return new(LeaveAttributeMatchStatus.Invalid, []);

            if (expectedSignature.Matches(candidateSignature))
                matches.Add(candidate);
        }

        return CreateMatchResult(matches);
    }

    /// <summary>
    /// Находит кандидатов по точному полному набору нерuntime-атрибутов.
    /// </summary>
    /// <param name="expectedValues">Независимые черновики ожидаемых значений.</param>
    /// <param name="candidates">Кандидаты для поиска.</param>
    /// <returns>Результат поиска или невалидный результат при ошибке значения.</returns>
    public LeaveAttributeMatchResult FindMatches(
        IEnumerable<LeaveAttributeValueDraft> expectedValues,
        IEnumerable<TreeLeaveModel> candidates)
    {
        ArgumentNullException.ThrowIfNull(expectedValues);
        ArgumentNullException.ThrowIfNull(candidates);

        var drafts = expectedValues.ToArray();
        if (drafts.GroupBy(x => x.DeclaringUuid).Any(x => x.Count() > 1))
            return new(LeaveAttributeMatchStatus.Invalid, []);
        var comparedDrafts = drafts.Where(x => x.MatchesAnyValue == false).ToArray();
        var expectedSignature = LeaveAttributeValueSignature.Create(comparedDrafts);
        if (expectedSignature.IsValid == false)
            return new(LeaveAttributeMatchStatus.Invalid, []);

        var usesWildcardCriteria = drafts.Any(x => x.MatchesAnyValue);
        var declaringUuids = expectedSignature.DeclaringUuids.ToHashSet();
        var matches = new List<TreeLeaveModel>();
        foreach (var candidate in candidates)
        {
            ArgumentNullException.ThrowIfNull(candidate);

            var candidateAttributes = candidate.Attributes.Where(x =>
                x.IsRuntime == false
                && (usesWildcardCriteria == false || declaringUuids.Contains(x.DeclaringUuid)));
            var candidateSignature = LeaveAttributeValueSignature.Create(candidateAttributes);
            if (candidateSignature.IsValid == false)
                return new(LeaveAttributeMatchStatus.Invalid, []);

            if (expectedSignature.Matches(candidateSignature))
                matches.Add(candidate);
        }

        return CreateMatchResult(matches);
    }

    private static LeaveAttributeMatchResult CreateMatchResult(
        IReadOnlyList<TreeLeaveModel> matches)
    {
        var status = matches.Count switch
        {
            0 => LeaveAttributeMatchStatus.NotFound,
            1 => LeaveAttributeMatchStatus.Resolved,
            _ => LeaveAttributeMatchStatus.Ambiguous
        };
        return new(status, matches);
    }
}
