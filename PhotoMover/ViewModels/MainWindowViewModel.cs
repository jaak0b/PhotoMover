namespace PhotoMover.ViewModels;

using PhotoMover.Core.Services;
using PhotoMover.Core.Models;
using System.Collections.ObjectModel;
using System.Windows.Input;

/// <summary>
/// Main application view model coordinating all features.
/// </summary>
public sealed class MainWindowViewModel : ViewModelBase
{
    private readonly IRuleRepository _ruleRepository;

    private ObservableCollection<GroupingRule> _rules = new();
    private GroupingRule? _selectedRule;
    private string _appTitle = "PhotoMover";
    private int _selectedTabIndex = 0;

    public RuleEditorViewModel RuleEditorViewModel { get; }
    public SdImportViewModel SdImportViewModel { get; }
    public FtpServerViewModel FtpServerViewModel { get; }

    public ObservableCollection<GroupingRule> Rules
    {
        get => _rules;
        private set => SetProperty(ref _rules, value);
    }

    public GroupingRule? SelectedRule
    {
        get => _selectedRule;
        set => SetProperty(ref _selectedRule, value);
    }

    public string AppTitle
    {
        get => _appTitle;
        private set => SetProperty(ref _appTitle, value);
    }

    public int SelectedTabIndex
    {
        get => _selectedTabIndex;
        set => SetProperty(ref _selectedTabIndex, value);
    }

    public ICommand NavigateToFtpCommand { get; }
    public ICommand NavigateToSdImportCommand { get; }
    public ICommand NavigateToRulesCommand { get; }

    public MainWindowViewModel(
        RuleEditorViewModel ruleEditorViewModel,
        SdImportViewModel sdImportViewModel,
        FtpServerViewModel ftpServerViewModel,
        IRuleRepository ruleRepository)
    {
        RuleEditorViewModel = ruleEditorViewModel ?? throw new ArgumentNullException(nameof(ruleEditorViewModel));
        SdImportViewModel = sdImportViewModel ?? throw new ArgumentNullException(nameof(sdImportViewModel));
        FtpServerViewModel = ftpServerViewModel ?? throw new ArgumentNullException(nameof(ftpServerViewModel));
        _ruleRepository = ruleRepository ?? throw new ArgumentNullException(nameof(ruleRepository));

        NavigateToFtpCommand = new RelayCommand(_ => SelectedTabIndex = 1);
        NavigateToSdImportCommand = new RelayCommand(_ => SelectedTabIndex = 2);
        NavigateToRulesCommand = new RelayCommand(_ => SelectedTabIndex = 3);
    }

    public async Task InitializeAsync()
    {
        await LoadRulesAsync();
    }

    public async Task LoadRulesAsync()
    {
        try
        {
            // Load rules from repository into RuleEditorViewModel
            await RuleEditorViewModel.LoadRulesAsync();
        }
        catch
        {
            // Handle error silently or log
        }
    }
}
