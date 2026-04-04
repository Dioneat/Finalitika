using CommunityToolkit.Mvvm.Messaging;
using Finalitika10.Views;
using Finalitika10.Views.PlanPages;

namespace Finalitika10
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();

            Routing.RegisterRoute("SettingsPage", typeof(SettingsPage));

            UpdateDynamicTab();

            // Подписываемся на сообщения из настроек, чтобы менять вкладку на лету
            WeakReferenceMessenger.Default.Register<TabUpdateMessage>(this, (r, m) =>
            {
                UpdateDynamicTab();
            });

            Routing.RegisterRoute(nameof(DocumentEditorPage), typeof(DocumentEditorPage));
            Routing.RegisterRoute(nameof(PdfPreviewPage), typeof(PdfPreviewPage));
            Routing.RegisterRoute(nameof(DocumentsPage), typeof(DocumentsPage));
            Routing.RegisterRoute(nameof(SettingsPage), typeof(SettingsPage));
            Routing.RegisterRoute("AllAssetsPage", typeof(AllAssetsPage));
            Routing.RegisterRoute("AllOperationsPage", typeof(AllOperationsPage));
            Routing.RegisterRoute("AddCurrencyPage", typeof(AddCurrencyPage));
            Routing.RegisterRoute("EditJobProfilePage", typeof(EditJobProfilePage));
            Routing.RegisterRoute(nameof(NotePage), typeof(NotePage));
            Routing.RegisterRoute(nameof(AllNotesPage), typeof(Views.AllNotesPage));
            Routing.RegisterRoute(nameof(AllPaymentsPage), typeof(AllPaymentsPage));
            Routing.RegisterRoute("ImportPage", typeof(Views.ImportPage));
            Routing.RegisterRoute(nameof(AddPaymentPage), typeof(AddPaymentPage));
            Routing.RegisterRoute("AllProjectsPage", typeof(AllProjectsPage));
            Routing.RegisterRoute("ProjectDetailPage", typeof(ProjectDetailPage));
            Routing.RegisterRoute("AccountsPage", typeof(Views.AccountsPage));
            Routing.RegisterRoute("CategoriesPage", typeof(Views.CategoriesPage));


        }
        public void UpdateDynamicTab()
        {
            bool useDocs = Preferences.Default.Get("UseDocsTab", false);

            if (useDocs)
            {
                DynamicTab.Title = "Документы";
                DynamicTab.Icon = "documents.png"; 
                DynamicContent.ContentTemplate = new DataTemplate(typeof(DocumentsPage));
            }
            else
            {
                DynamicTab.Title = "Инвестиции";
                DynamicTab.Icon = "investment.png";
                DynamicContent.ContentTemplate = new DataTemplate(typeof(InvestmentsPage));
            }
        }
    }
}
