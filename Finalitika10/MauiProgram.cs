using CommunityToolkit.Maui;
using CommunityToolkit.Mvvm.Messaging;
using Finalitika10.Services;
using Finalitika10.Services.AppServices;
using Finalitika10.Services.DocumentsServices;
using Finalitika10.Services.Import;
using Finalitika10.Services.Interfaces.DocumentsService;
using Finalitika10.Services.Investments;
using Finalitika10.Services.PlanServices;
using Finalitika10.ViewModels;
using Finalitika10.ViewModels.AnalysisViewModels;
using Finalitika10.ViewModels.PlanViewModels;
using Finalitika10.Views;
using Finalitika10.Views.PlanPages;
using Microcharts.Maui;
using Microsoft.Extensions.Logging;
using Plugin.Fingerprint;
using Plugin.LocalNotification;

namespace Finalitika10
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();

            ConfigureApp(builder);
            ConfigurePlatform(builder);
            RegisterInfrastructure(builder.Services);
            RegisterAppServices(builder.Services);
            RegisterFinanceServices(builder.Services);
            RegisterPlanServices(builder.Services);
            RegisterInvestmentServices(builder.Services);
            RegisterDocumentServices(builder.Services);
            RegisterViewModels(builder.Services);
            RegisterPages(builder.Services);
            ConfigureLogging(builder);

            return builder.Build();
        }

        private static void ConfigureApp(MauiAppBuilder builder)
        {
            builder
                .UseMauiApp<App>()
                .UseMauiCommunityToolkit()
                .UseMicrocharts()
                .UseLocalNotification()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });
        }

        private static void ConfigurePlatform(MauiAppBuilder builder)
        {
#if ANDROID
            CrossFingerprint.SetCurrentActivityResolver(() => Platform.CurrentActivity);
#endif
        }

        private static void RegisterInfrastructure(IServiceCollection services)
        {
            string dbPath = Path.Combine(FileSystem.AppDataDirectory, "finalitika_core.db3");

            services.AddSingleton(_ => new FinalitikaDatabase(dbPath));
            services.AddSingleton<IMessenger>(WeakReferenceMessenger.Default);
        }

        private static void RegisterAppServices(IServiceCollection services)
        {
            services.AddSingleton<IPreferencesService, PreferencesService>();
            services.AddSingleton<ISecureStorageService, SecureStorageService>();
            services.AddSingleton<IDialogService, DialogService>();
            services.AddSingleton<IHapticService, HapticService>();
            services.AddSingleton<IAppInfoService, AppInfoService>();
        }
        private static void RegisterFinanceServices(IServiceCollection services)
        {
            services.AddSingleton<ITransactionService, TransactionService>();
            services.AddSingleton<ICategoryService, CategoryService>();
            services.AddSingleton<IAccountService, AccountService>();
            services.AddSingleton<CategoryMappingService>();
            services.AddSingleton<INotesService, NotesService>();
            services.AddSingleton<IJobProfileService, JobProfileService>();
        }

        private static void RegisterPlanServices(IServiceCollection services)
        {
            services.AddSingleton<IProjectService, ProjectService>();
            services.AddSingleton<IPaymentService, PaymentService>();
        }

        private static void RegisterInvestmentServices(IServiceCollection services)
        {
            services.AddSingleton<IInvestmentService, TinkoffInvestmentService>();

            services.AddHttpClient<ICurrencyService, CbrCurrencyService>();
            services.AddHttpClient<ICentralBankService, CentralBankService>();
            services.AddHttpClient<IMarketMoodService, MarketMoodService>();
        }

        private static void RegisterDocumentServices(IServiceCollection services)
        {
            services.AddSingleton<INameDeclensionService, PetrovichDeclensionService>();

            services.AddTransient<IDocumentGeneratorStrategy, PdfGeneratorStrategy>();
            services.AddTransient<IDocumentGeneratorStrategy, XmlGeneratorStrategy>();
            services.AddTransient<GenerateDocumentUseCase>();
        }

        private static void RegisterViewModels(IServiceCollection services)
        {
            services.AddTransient<MainViewModel>();
            services.AddTransient<ProfileViewModel>();
            services.AddTransient<SettingsViewModel>();

            services.AddTransient<AccountsViewModel>();
            services.AddTransient<AddEditAccountViewModel>();
            services.AddTransient<CategoriesViewModel>();
            services.AddTransient<AddEditCategoryViewModel>();
            services.AddTransient<ImportViewModel>();
            services.AddTransient<AnalysisViewModel>();

            services.AddTransient<InvestmentsViewModel>();
            services.AddTransient<AddCurrencyViewModel>();
            services.AddTransient<AllAssetsViewModel>();
            services.AddTransient<AllOperationsViewModel>();

            services.AddTransient<PlansViewModel>();
            services.AddTransient<AllProjectsViewModel>();
            services.AddTransient<ProjectDetailViewModel>();
            services.AddTransient<AddProjectViewModel>();
            services.AddTransient<AllPaymentsViewModel>();
            services.AddTransient<AddPaymentViewModel>();
            services.AddTransient<SalaryCalculatorViewModel>();

            services.AddTransient<AllNotesViewModel>();
            services.AddTransient<NoteViewModel>();

            services.AddTransient<DocumentsViewModel>();
            services.AddTransient<PersonalDataViewModel>();
            services.AddTransient<DocumentEditorViewModel>();
            services.AddTransient<PdfPreviewViewModel>();
            services.AddTransient<EditJobProfileViewModel>();
        }

        private static void RegisterPages(IServiceCollection services)
        {
            services.AddTransient<MainPage>();
            services.AddTransient<ProfilePage>();
            services.AddTransient<SettingsPage>();

            services.AddTransient<AccountsPage>();
            services.AddTransient<AddEditAccountPage>();
            services.AddTransient<CategoriesPage>();
            services.AddTransient<AddEditCategoryPage>();
            services.AddTransient<ImportPage>();
            services.AddTransient<FinancialAnalysisPage>();

            services.AddTransient<InvestmentsPage>();
            services.AddTransient<AddCurrencyPage>();
            services.AddTransient<AllAssetsPage>();
            services.AddTransient<AllOperationsPage>();

            services.AddTransient<PlansPage>();
            services.AddTransient<AllProjectsPage>();
            services.AddTransient<ProjectDetailPage>();
            services.AddTransient<AddProjectPage>();
            services.AddTransient<AllPaymentsPage>();
            services.AddTransient<AddPaymentPage>();
            services.AddTransient<SalaryCalculatorPage>();

            services.AddTransient<AllNotesPage>();
            services.AddTransient<NotePage>();


            services.AddTransient<DocumentsPage>();
            services.AddTransient<PersonalDataPage>();
            services.AddTransient<DocumentEditorPage>();
            services.AddTransient<PdfPreviewPage>();
            services.AddTransient<EditJobProfilePage>();
        }

        private static void ConfigureLogging(MauiAppBuilder builder)
        {
#if DEBUG
            builder.Logging.AddDebug();
#endif
        }
    }
}