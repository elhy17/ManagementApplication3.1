namespace DidiApp.ViewModels;

public sealed class MainViewModel : ViewModelBase
{
    private ViewModelBase _currentPage;

    public MainViewModel()
    {
        TasksPage = new TasksViewModel();
        FocusPage = new FocusViewModel();
        InsightsPage = new InsightsViewModel();
        JournalPage = new JournalViewModel();
        GoalsPage = new GoalsViewModel();
        SettingsPage = new SettingsViewModel();

        _currentPage = TasksPage;

        NavTasks = new RelayCommand(() => CurrentPage = TasksPage);
        NavFocus = new RelayCommand(() => { FocusPage.RefreshTasks(); CurrentPage = FocusPage; });
        NavInsights = new RelayCommand(() => { InsightsPage.RefreshChart(); CurrentPage = InsightsPage; });
        NavJournal = new RelayCommand(() => CurrentPage = JournalPage);
        NavGoals = new RelayCommand(() => CurrentPage = GoalsPage);
        NavSettings = new RelayCommand(() => CurrentPage = SettingsPage);
    }

    public ViewModelBase CurrentPage { get => _currentPage; set => SetProperty(ref _currentPage, value); }
    
    public TasksViewModel TasksPage { get; }
    public FocusViewModel FocusPage { get; }
    public InsightsViewModel InsightsPage { get; }
    public JournalViewModel JournalPage { get; }
    public GoalsViewModel GoalsPage { get; }
    public SettingsViewModel SettingsPage { get; }

    public RelayCommand NavTasks { get; }
    public RelayCommand NavFocus { get; }
    public RelayCommand NavInsights { get; }
    public RelayCommand NavJournal { get; }
    public RelayCommand NavGoals { get; }
    public RelayCommand NavSettings { get; }
}
