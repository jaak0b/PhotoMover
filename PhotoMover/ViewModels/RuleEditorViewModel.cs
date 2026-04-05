namespace PhotoMover.ViewModels;

using PhotoMover.Core.Models;
using PhotoMover.Core.Services;
using System.Collections.ObjectModel;
using System.Windows.Input;

public sealed class RuleEditorViewModel : ViewModelBase
{
    private readonly IGroupingRuleEngine _groupingRuleEngine;
    private readonly IRuleRepository _ruleRepository;
    private readonly IMetadataExtractor _metadataExtractor;

    private ObservableCollection<GroupingRule> _rules = new();
    private GroupingRule? _selectedRule;
    private string _ruleName = "";
    private string _pathPattern = "{CameraModel}/{DateTaken:yyyy}/{DateTaken:MM}";
    private string? _previewPath;
    private bool _isSaving;
    private bool _isEditMode;
    private string? _editingRuleId;

    public ObservableCollection<GroupingRule> Rules
    {
        get => _rules;
        private set => SetProperty(ref _rules, value);
    }

    public bool IsEditControlsEnabled => Rules != null && Rules.Count > 0;

    public GroupingRule? SelectedRule
    {
        get => _selectedRule;
        set
        {
            if (SetProperty(ref _selectedRule, value))
            {
                if (value != null)
                {
                    LoadRuleForEditing(value);
                }
                else
                {
                    IsEditMode = false;
                    _editingRuleId = null;
                    RuleName = "";
                    PathPattern = "{CameraModel}/{DateTaken:yyyy}/{DateTaken:MM}";
                    PreviewPath = null;
                }

                OnPropertyChanged(nameof(IsEditControlsEnabled));
            }
        }
    }

    public string RuleName
    {
        get => _ruleName;
        set => SetProperty(ref _ruleName, value);
    }

    public string PathPattern
    {
        get => _pathPattern;
        set
        {
            if (SetProperty(ref _pathPattern, value))
            {
                ValidateAndUpdatePreview();
            }
        }
    }

    public string? PreviewPath
    {
        get => _previewPath;
        private set => SetProperty(ref _previewPath, value);
    }

    public bool IsSaving
    {
        get => _isSaving;
        private set => SetProperty(ref _isSaving, value);
    }

    public bool IsEditMode
    {
        get => _isEditMode;
        private set => SetProperty(ref _isEditMode, value);
    }

    public IReadOnlyCollection<string> AvailablePlaceholders => _groupingRuleEngine.GetAvailablePlaceholders();

    public ICommand SaveRuleCommand { get; }
    public ICommand AddNewRuleCommand { get; }
    public ICommand DeleteRuleCommand { get; }
    public ICommand SetActiveRuleCommand { get; }
    public bool CanEditConfiguration => !AppState.IsImporting && !AppState.IsFtpRunning;

    public RuleEditorViewModel(
        IGroupingRuleEngine groupingRuleEngine,
        IRuleRepository ruleRepository,
        IMetadataExtractor metadataExtractor)
    {
        _groupingRuleEngine = groupingRuleEngine ?? throw new ArgumentNullException(nameof(groupingRuleEngine));
        _ruleRepository = ruleRepository ?? throw new ArgumentNullException(nameof(ruleRepository));
        _metadataExtractor = metadataExtractor ?? throw new ArgumentNullException(nameof(metadataExtractor));

        SaveRuleCommand = new RelayCommandAsync(_ => SaveRuleAsync(), _ => !IsSaving && CanEditConfiguration);
        AddNewRuleCommand = new RelayCommand(_ => AddNewRule(), _ => CanEditConfiguration);
        DeleteRuleCommand = new RelayCommandAsync(_ => DeleteSelectedRuleAsync(), _ => SelectedRule != null && !IsSaving && CanEditConfiguration);
        SetActiveRuleCommand = new RelayCommandAsync(_ => SetSelectedRuleActiveAsync(), _ => SelectedRule != null && !SelectedRule.IsActive && !IsSaving && CanEditConfiguration);

        AppState.ImportingChanged += (_, _) =>
        {
            OnPropertyChanged(nameof(CanEditConfiguration));
            System.Windows.Input.CommandManager.InvalidateRequerySuggested();
        };
        AppState.FtpRunningChanged += (_, _) =>
        {
            OnPropertyChanged(nameof(CanEditConfiguration));
            System.Windows.Input.CommandManager.InvalidateRequerySuggested();
        };

        ValidateAndUpdatePreview();
    }

    public async Task LoadRulesAsync()
    {
        try
        {
            var rules = await _ruleRepository.GetAllRulesAsync();
            Rules.Clear();

            var activeRule = await _ruleRepository.GetActiveRuleAsync();

            foreach (var rule in rules.OrderBy(r => r.Priority).ThenBy(r => r.Name))
            {
                var adjusted = rule;
                if (activeRule != null)
                {
                    adjusted = rule with { IsActive = rule.Id == activeRule.Id };
                }

                Rules.Add(adjusted);
            }

            // Restore selection: prefer the rule we were editing, otherwise select the active rule
            GroupingRule? toSelect = null;
            if (!string.IsNullOrEmpty(_editingRuleId))
            {
                toSelect = Rules.FirstOrDefault(r => r.Id == _editingRuleId);
            }

            if (toSelect == null && activeRule != null)
            {
                toSelect = Rules.FirstOrDefault(r => r.Id == activeRule.Id);
            }

            SelectedRule = toSelect; // will be null if nothing to select

            OnPropertyChanged(nameof(IsEditControlsEnabled));
            OnPropertyChanged(nameof(CanEditConfiguration));
        }
        catch (Exception ex)
        {
            PreviewPath = $"Error loading rules: {ex.Message}";
        }
    }

    public async Task SaveRuleAsync()
    {
        if (string.IsNullOrWhiteSpace(RuleName))
        {
            PreviewPath = "Rule name cannot be empty";
            return;
        }

        if (!_groupingRuleEngine.ValidatePattern(PathPattern))
        {
            PreviewPath = "Invalid pattern - check your placeholders";
            return;
        }

            IsSaving = true;
        try
        {
            // Snapshot whether we are creating vs updating before any async work changes state
            bool isCreatingNew = SelectedRule == null && !IsEditMode;
            string? existingId = SelectedRule?.Id ?? (IsEditMode ? _editingRuleId : null);

            GroupingRule rule;
            if (existingId != null)
            {
                var existingRule = await _ruleRepository.GetRuleByIdAsync(existingId);
                if (existingRule == null)
                {
                    PreviewPath = "Rule not found";
                    return;
                }

                rule = existingRule with
                {
                    Name = RuleName,
                    PathPattern = PathPattern
                };
            }
            else
            {
                rule = new GroupingRule
                {
                    Id = Guid.NewGuid().ToString(),
                    Name = RuleName,
                    PathPattern = PathPattern,
                    IsActive = false,
                    Priority = 0,
                    Metadata = new Dictionary<string, string>()
                };
            }

            await _ruleRepository.SaveRuleAsync(rule);

            // New rules become active automatically
            if (isCreatingNew)
            {
                await _ruleRepository.SetActiveRuleAsync(rule.Id);
            }

            string savedId = rule.Id;
            PreviewPath = "Rule saved successfully!";
            await LoadRulesAsync();

            // Re-select the saved rule so edits remain visible
            var saved = Rules.FirstOrDefault(r => r.Id == savedId);
            if (saved != null)
            {
                SelectedRule = saved;
            }
        }
        catch (Exception ex)
        {
            PreviewPath = $"Error saving rule: {ex.Message}";
        }
        finally
        {
            IsSaving = false;
        }
    }

    public void AddNewRule()
    {
        if (!CanEditConfiguration)
        {
            PreviewPath = "Cannot add rule while import or FTP server is running";
            return;
        }

        IsEditMode = false;
        _editingRuleId = null;
        RuleName = "";
        PathPattern = "{CameraModel}/{DateTaken:yyyy}/{DateTaken:MM}";
        SelectedRule = null;
        PreviewPath = null;
        ValidateAndUpdatePreview();
    }

    private void LoadRuleForEditing(GroupingRule rule)
    {
        IsEditMode = true;
        _editingRuleId = rule.Id;
        RuleName = rule.Name;
        PathPattern = rule.PathPattern;
        ValidateAndUpdatePreview();
    }

    public async Task DeleteSelectedRuleAsync()
    {
        if (SelectedRule == null)
            return;

        if (!CanEditConfiguration)
        {
            PreviewPath = "Cannot delete rule while import or FTP server is running";
            return;
        }

        // Show confirmation dialog
        var result = System.Windows.MessageBox.Show(
            $"Are you sure you want to delete the rule '{SelectedRule.Name}'?",
            "Delete Rule",
            System.Windows.MessageBoxButton.YesNo,
            System.Windows.MessageBoxImage.Question);

        if (result != System.Windows.MessageBoxResult.Yes)
            return;

        IsSaving = true;
        try
        {
            await _ruleRepository.DeleteRuleAsync(SelectedRule.Id);
            PreviewPath = "Rule deleted successfully!";
            await LoadRulesAsync();
            AddNewRule();
        }
        catch (Exception ex)
        {
            PreviewPath = $"Error deleting rule: {ex.Message}";
        }
        finally
        {
            IsSaving = false;
        }
    }

    public async Task SetSelectedRuleActiveAsync()
    {
        if (SelectedRule == null)
            return;

        if (!CanEditConfiguration)
        {
            PreviewPath = "Cannot change active rule while import or FTP server is running";
            return;
        }

        IsSaving = true;
        try
        {
            await _ruleRepository.SetActiveRuleAsync(SelectedRule.Id);
            PreviewPath = "Rule set as active!";
            await LoadRulesAsync();
        }
        catch (Exception ex)
        {
            PreviewPath = $"Error setting active rule: {ex.Message}";
        }
        finally
        {
            IsSaving = false;
        }
    }

    public void InsertPlaceholder(string placeholder)
    {
        PathPattern += $"{{{placeholder}}}";
    }

    private void ValidateAndUpdatePreview()
    {
        if (!_groupingRuleEngine.ValidatePattern(PathPattern))
        {
            PreviewPath = "Invalid pattern";
            return;
        }

        // Create a sample metadata for preview
        var sampleMetadata = new PhotoMetadata
        {
            FilePath = "/sample/photo.jpg",
            FileName = "photo.jpg",
            DateTaken = DateTime.Now,
            CameraModel = "Sony A7R V",
            LensModel = "24-70mm",
            Orientation = 1,
            AllMetadata = new Dictionary<string, string>(),
            TagIdMetadata = new Dictionary<string, string>
            {
                ["0x0110"] = "Sony A7R V",  // Model tag ID
                ["0x9003"] = DateTime.Now.ToString("yyyy:MM:dd HH:mm:ss")  // DateTaken tag ID
            }
        };

        var result = _groupingRuleEngine.EvaluatePattern(PathPattern, sampleMetadata);
        PreviewPath = result ?? "Preview error";
    }
}
