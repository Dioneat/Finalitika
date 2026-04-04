using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Finalitika10.Models;
using Finalitika10.Services;
using Finalitika10.Services.PlanServices;
using Finalitika10.Views;
using Finalitika10.Views.PlanPages;
using System.Collections.ObjectModel;

namespace Finalitika10.ViewModels.PlanViewModels
{
    public record NotesUpdatedMessage();
    public record ProjectUpdatedMessage();

    public partial class PlansViewModel : ObservableObject
    {
        private const int NotesPreviewCount = 2;
        private const int PaymentsPreviewCount = 3;
        private const int ProjectsPreviewCount = 5;

        private readonly IJobProfileService _jobProfileService;
        private readonly INotesService _notesService;
        private readonly IPaymentService _paymentService;
        private readonly IProjectService _projectService;

        private readonly SemaphoreSlim _refreshLock = new(1, 1);
        private CancellationTokenSource? _refreshCts;

        public ObservableCollection<FinancialProject> Projects { get; } = new();
        public ObservableCollection<LifeScenario> Scenarios { get; } = new();
        public ObservableCollection<PaymentReminder> Payments { get; } = new();
        public ObservableCollection<ProjectNote> Notes { get; } = new();

        [ObservableProperty]
        private decimal plannedIncome = 150000m;

        [ObservableProperty]
        private decimal allocatedNeeds = 70000m;

        [ObservableProperty]
        private decimal allocatedSavings = 33300m;

        [ObservableProperty]
        private decimal unallocated = 46700m;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(FabIcon))]
        private bool isFabMenuOpen;

        public string FabIcon => IsFabMenuOpen ? "✕" : "+";

        public PlansViewModel(
            IJobProfileService jobProfileService,
            INotesService notesService,
            IPaymentService paymentService,
            IProjectService projectService)
        {
            _jobProfileService = jobProfileService;
            _notesService = notesService;
            _paymentService = paymentService;
            _projectService = projectService;

            LoadScenarios();
            RegisterMessages();
            RequestDataUpdate();
        }

        private void RegisterMessages()
        {
            WeakReferenceMessenger.Default.Register<NotesUpdatedMessage>(this, (_, _) => RequestDataUpdate());
            WeakReferenceMessenger.Default.Register<PaymentAddedMessage>(this, (_, _) => RequestDataUpdate());
            WeakReferenceMessenger.Default.Register<ProjectUpdatedMessage>(this, (_, _) => RequestDataUpdate());

            WeakReferenceMessenger.Default.Register<SalaryCalculatedMessage>(this, (_, message) =>
            {
                decimal totalIncome = Convert.ToDecimal(message.MainSalaryAmount);

                if (message.HasAdvance)
                {
                    totalIncome += Convert.ToDecimal(message.AdvanceAmount);
                }

                PlannedIncome = totalIncome;
                Unallocated = PlannedIncome - AllocatedNeeds - AllocatedSavings;
            });
        }

        private void RequestDataUpdate()
        {
            _ = RefreshAsync();
        }

        public async Task RefreshAsync()
        {
            var newCts = new CancellationTokenSource();
            var oldCts = Interlocked.Exchange(ref _refreshCts, newCts);

            oldCts?.Cancel();
            oldCts?.Dispose();

            try
            {
                await Task.Delay(100, newCts.Token);
                await _refreshLock.WaitAsync(newCts.Token);

                try
                {
                    var notes = await Task.Run(() =>
                        _notesService
                            .GetAllNotes()
                            .OrderByDescending(n => n.CreatedAt)
                            .Take(NotesPreviewCount)
                            .ToList(), newCts.Token);

                    var payments = await Task.Run(() =>
                        _paymentService
                            .GetAllPayments()
                            .OrderBy(p => p.MonthlyDay)
                            .Take(PaymentsPreviewCount)
                            .ToList(), newCts.Token);

                    var projects = await Task.Run(() =>
                        _projectService
                            .GetAllProjects()
                            .Take(ProjectsPreviewCount)
                            .ToList(), newCts.Token);

                    await MainThread.InvokeOnMainThreadAsync(() =>
                    {
                        ReplaceRange(Notes, notes);
                        ReplaceRange(Payments, payments);
                        ReplaceRange(Projects, projects);
                    });
                }
                finally
                {
                    _refreshLock.Release();
                }
            }
            catch (OperationCanceledException)
            {
            }
        }

        private static void ReplaceRange<T>(ObservableCollection<T> collection, IEnumerable<T> items)
        {
            collection.Clear();

            foreach (var item in items)
            {
                collection.Add(item);
            }
        }

        private void LoadScenarios()
        {
            Scenarios.Clear();

            Scenarios.Add(new LifeScenario
            {
                Icon = "🏠",
                Title = "Квартира через 5 лет",
                Description = "Чтобы накопить первоначальный взнос 2 млн ₽, откладывайте 33 300 ₽/мес под 7% годовых.",
                ActionText = "Создать инвест. портфель",
                ActionColor = "#27AE60"
            });

            Scenarios.Add(new LifeScenario
            {
                Icon = "👴",
                Title = "Пенсионный капитал",
                Description = "Для пассивного дохода 100 тыс. ₽/мес в 60 лет, ваш целевой капитал: 30 млн ₽.",
                ActionText = "Открыть калькулятор",
                ActionColor = "#2980B9"
            });
        }

        [RelayCommand]
        private void ToggleFabMenu()
        {
            IsFabMenuOpen = !IsFabMenuOpen;
        }

        [RelayCommand]
        private async Task AddProjectAsync()
        {
            IsFabMenuOpen = false;

            if (Shell.Current is null)
            {
                return;
            }

            var page = new AddProjectPage(new AddProjectViewModel(_projectService));
            await Shell.Current.Navigation.PushModalAsync(page);
        }

        [RelayCommand]
        private async Task AddNoteAsync()
        {
            IsFabMenuOpen = false;

            if (Shell.Current is null)
            {
                return;
            }

            await Shell.Current.GoToAsync("NotePage");
        }

        [RelayCommand]
        private async Task AddPaymentAsync()
        {
            IsFabMenuOpen = false;

            if (Shell.Current is null)
            {
                return;
            }

            var page = new AddPaymentPage(new AddPaymentViewModel(_paymentService, null));
            await Shell.Current.Navigation.PushModalAsync(page);
        }

        [RelayCommand]
        private async Task ViewAllProjectsAsync()
        {
            if (Shell.Current is null)
            {
                return;
            }

            await Shell.Current.GoToAsync("AllProjectsPage");
        }

        [RelayCommand]
        private async Task ViewAllPaymentsAsync()
        {
            if (Shell.Current is null)
            {
                return;
            }

            await Shell.Current.GoToAsync("AllPaymentsPage");
        }

        [RelayCommand]
        private async Task ViewAllNotesAsync()
        {
            if (Shell.Current is null)
            {
                return;
            }

            await Shell.Current.GoToAsync("AllNotesPage");
        }

        [RelayCommand]
        private async Task OpenProjectAsync(FinancialProject? project)
        {
            if (project is null || Shell.Current is null)
            {
                return;
            }

            await Shell.Current.GoToAsync($"ProjectDetailPage?ProjectId={project.Id}");
        }

        [RelayCommand]
        private async Task OpenNoteAsync(ProjectNote? note)
        {
            if (note is null || Shell.Current is null)
            {
                return;
            }

            await Shell.Current.GoToAsync($"NotePage?NoteId={note.Id}");
        }

        [RelayCommand]
        private async Task CalculateSalaryAsync()
        {
            if (Shell.Current is null)
            {
                return;
            }

            var page = new SalaryCalculatorPage(new SalaryCalculatorViewModel(_jobProfileService));
            await Shell.Current.Navigation.PushModalAsync(page);
        }
    }
}