namespace PhotoMover.Core.Services;

using PhotoMover.Core.Models;

/// <summary>
/// Service for persisting and retrieving grouping rules.
/// </summary>
public interface IRuleRepository
{
    /// <summary>
    /// Gets all stored grouping rules.
    /// </summary>
    Task<IReadOnlyCollection<GroupingRule>> GetAllRulesAsync();

    /// <summary>
    /// Gets a specific grouping rule by ID.
    /// </summary>
    Task<GroupingRule?> GetRuleByIdAsync(string ruleId);

    /// <summary>
    /// Saves a grouping rule.
    /// </summary>
    Task SaveRuleAsync(GroupingRule rule);

    /// <summary>
    /// Deletes a grouping rule.
    /// </summary>
    Task DeleteRuleAsync(string ruleId);

    /// <summary>
    /// Gets the active/default grouping rule.
    /// </summary>
    Task<GroupingRule?> GetActiveRuleAsync();

    /// <summary>
    /// Sets a rule as the active/default rule.
    /// </summary>
    Task SetActiveRuleAsync(string ruleId);
}
