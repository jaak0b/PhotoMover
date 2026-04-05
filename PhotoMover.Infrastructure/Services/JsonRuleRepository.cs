namespace PhotoMover.Infrastructure.Services;

using PhotoMover.Core.Services;
using PhotoMover.Core.Models;
using System.Text.Json;
using System.Text.Json.Serialization;

public sealed class JsonRuleRepository : IRuleRepository
{
    private readonly string _storagePath;
    private readonly IFileSystem _fileSystem;

    public JsonRuleRepository(IFileSystem fileSystem, string storagePath = "rules")
    {
        _fileSystem = fileSystem ?? throw new ArgumentNullException(nameof(fileSystem));
        _storagePath = storagePath;
        _fileSystem.CreateDirectory(_storagePath);
    }

    public async Task<IReadOnlyCollection<GroupingRule>> GetAllRulesAsync()
    {
        return await Task.Run(() =>
        {
            var rules = new List<GroupingRule>();
            var files = _fileSystem.GetFiles(_storagePath, "*.json");

            // Determine the active rule id from storage (if any)
            string? activeId = null;
            var activeFilePath = Path.Combine(_storagePath, "active.txt");
            if (_fileSystem.FileExists(activeFilePath))
            {
                try
                {
                    activeId = File.ReadAllText(activeFilePath).Trim();
                }
                catch
                {
                    activeId = null;
                }
            }

            foreach (var file in files)
            {
                var rule = DeserializeRule(file);
                if (rule != null)
                    rules.Add(rule);
            }

            // Ensure only the rule matching activeId is marked as active.
            if (!string.IsNullOrEmpty(activeId))
            {
                for (int i = 0; i < rules.Count; i++)
                {
                    var r = rules[i];
                    if (r.Id == activeId && !r.IsActive)
                    {
                        rules[i] = r with { IsActive = true };
                    }
                    else if (r.Id != activeId && r.IsActive)
                    {
                        rules[i] = r with { IsActive = false };
                    }
                }
            }

            return rules;
        });
    }

    public async Task<GroupingRule?> GetRuleByIdAsync(string ruleId)
    {
        return await Task.Run(() =>
        {
            var filePath = Path.Combine(_storagePath, $"{ruleId}.json");
            return DeserializeRule(filePath);
        });
    }

    public async Task SaveRuleAsync(GroupingRule rule)
    {
        await Task.Run(() =>
        {
            var filePath = Path.Combine(_storagePath, $"{rule.Id}.json");
            var json = JsonSerializer.Serialize(rule, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(filePath, json);
        });
    }

    public async Task DeleteRuleAsync(string ruleId)
    {
        await Task.Run(() =>
        {
            var filePath = Path.Combine(_storagePath, $"{ruleId}.json");
            if (_fileSystem.FileExists(filePath))
                File.Delete(filePath);
        });
    }

    public async Task<GroupingRule?> GetActiveRuleAsync()
    {
        return await Task.Run(() =>
        {
            var activeFilePath = Path.Combine(_storagePath, "active.txt");
            if (!_fileSystem.FileExists(activeFilePath))
                return null;

            var activeId = File.ReadAllText(activeFilePath).Trim();
            return DeserializeRule(Path.Combine(_storagePath, $"{activeId}.json"));
        });
    }

    public async Task SetActiveRuleAsync(string ruleId)
    {
        await Task.Run(() =>
        {
            var activeFilePath = Path.Combine(_storagePath, "active.txt");
            File.WriteAllText(activeFilePath, ruleId);
        });
    }

    private GroupingRule? DeserializeRule(string filePath)
    {
        try
        {
            if (!_fileSystem.FileExists(filePath))
                return null;

            var json = File.ReadAllText(filePath);
            return JsonSerializer.Deserialize<GroupingRule>(json);
        }
        catch
        {
            return null;
        }
    }
}
