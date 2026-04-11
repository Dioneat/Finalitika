using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Finalitika10.Models;
using Finalitika10.Services.Interfaces.DocumentsService;
using Finalitika10.Views;

namespace Finalitika10.ViewModels
{
    public static class DocTypes
    {
        public const string Resignation = "ResignationLetter";
        public const string Loan = "LoanAgreement";
        public const string Receipt = "MoneyReceipt";
        public const string Claim = "ClaimStatement";
        public const string PaidLeave = "PaidLeaveApplication";
        public const string UnpaidLeave = "UnpaidLeaveApplication";
        public const string Ndfl3 = "Ndfl3Declaration";
    }

    [QueryProperty(nameof(DocumentType), "DocType")]
    public partial class DocumentEditorViewModel : ObservableObject
    {
        private readonly GenerateDocumentUseCase _generateUseCase;
        private readonly INameDeclensionService _declensionService;

        public bool IsResignation => DocumentType == DocTypes.Resignation;
        public bool IsLoan => DocumentType == DocTypes.Loan;
        public bool IsReceipt => DocumentType == DocTypes.Receipt;
        public bool IsClaim => DocumentType == DocTypes.Claim;
        public bool IsPaidLeave => DocumentType == DocTypes.PaidLeave;
        public bool IsUnpaidLeave => DocumentType == DocTypes.UnpaidLeave;
        public bool IsNdfl3 => DocumentType == DocTypes.Ndfl3;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(IsResignation), nameof(IsLoan), nameof(IsReceipt), nameof(IsClaim), nameof(IsNdfl3), nameof(IsPaidLeave), nameof(IsUnpaidLeave))]
        [NotifyPropertyChangedFor(nameof(PageTitle), nameof(PageIcon))]
        private string documentType;

        [ObservableProperty] private bool isGenerating;

        public string PageTitle => DocumentType switch
        {
            DocTypes.Resignation => "Увольнение",
            DocTypes.Loan => "Договор займа",
            DocTypes.Receipt => "Расписка",
            DocTypes.Claim => "Исковое заявление",
            DocTypes.PaidLeave => "Оплачиваемый отпуск",
            DocTypes.UnpaidLeave => "Отпуск за свой счет",
            DocTypes.Ndfl3 => "Декларация 3-НДФЛ",
            _ => "Документ"
        };

        public string PageIcon => DocumentType switch
        {
            DocTypes.Ndfl3 => "🏛️",
            DocTypes.Loan => "🤝",
            DocTypes.Claim => "⚖️",
            _ => "📄"
        };

        // --- ПОЛЯ ДЛЯ УВОЛЬНЕНИЯ ---
        [ObservableProperty][NotifyCanExecuteChangedFor(nameof(CreateDocumentCommand))] private string directorName;
        [ObservableProperty][NotifyCanExecuteChangedFor(nameof(CreateDocumentCommand))] private string employeeName;
        [ObservableProperty][NotifyCanExecuteChangedFor(nameof(CreateDocumentCommand))] private string companyName;

        // --- ПОЛЯ ДЛЯ ДОГОВОРА ЗАЙМА ---
        [ObservableProperty][NotifyCanExecuteChangedFor(nameof(CreateDocumentCommand))] private string city = "г. Москва";
        [ObservableProperty][NotifyCanExecuteChangedFor(nameof(CreateDocumentCommand))] private string agreementDate = DateTime.Now.ToString("dd.MM.yyyy");
        [ObservableProperty][NotifyCanExecuteChangedFor(nameof(CreateDocumentCommand))] private string lenderName;
        [ObservableProperty][NotifyCanExecuteChangedFor(nameof(CreateDocumentCommand))] private string borrowerName;
        [ObservableProperty][NotifyCanExecuteChangedFor(nameof(CreateDocumentCommand))] private string transferDate;
        [ObservableProperty][NotifyCanExecuteChangedFor(nameof(CreateDocumentCommand))] private string loanAmount;
        [ObservableProperty][NotifyCanExecuteChangedFor(nameof(CreateDocumentCommand))] private string transferMethod = "наличными";
        [ObservableProperty][NotifyCanExecuteChangedFor(nameof(CreateDocumentCommand))] private string returnDate;
        [ObservableProperty][NotifyCanExecuteChangedFor(nameof(CreateDocumentCommand))] private string penaltyRate = "1%";
        [ObservableProperty][NotifyCanExecuteChangedFor(nameof(CreateDocumentCommand))] private string lenderRequisites;
        [ObservableProperty][NotifyCanExecuteChangedFor(nameof(CreateDocumentCommand))] private string borrowerRequisites;

        // --- ПОЛЯ ДЛЯ РАСПИСКИ ---
        [ObservableProperty][NotifyCanExecuteChangedFor(nameof(CreateDocumentCommand))] private string receiverName;
        [ObservableProperty][NotifyCanExecuteChangedFor(nameof(CreateDocumentCommand))] private string receiverBirthDate;
        [ObservableProperty][NotifyCanExecuteChangedFor(nameof(CreateDocumentCommand))] private string receiverPassport;
        [ObservableProperty][NotifyCanExecuteChangedFor(nameof(CreateDocumentCommand))] private string senderName;
        [ObservableProperty][NotifyCanExecuteChangedFor(nameof(CreateDocumentCommand))] private string senderBirthDate;
        [ObservableProperty][NotifyCanExecuteChangedFor(nameof(CreateDocumentCommand))] private string senderPassport;
        [ObservableProperty][NotifyCanExecuteChangedFor(nameof(CreateDocumentCommand))] private string receiptAmount;
        [ObservableProperty][NotifyCanExecuteChangedFor(nameof(CreateDocumentCommand))] private string receiptReturnDate;
        [ObservableProperty][NotifyCanExecuteChangedFor(nameof(CreateDocumentCommand))] private string receiptDate = DateTime.Now.ToString("dd.MM.yyyy");

        // --- ПОЛЯ ДЛЯ ИСКОВОГО ЗАЯВЛЕНИЯ ---
        [ObservableProperty][NotifyCanExecuteChangedFor(nameof(CreateDocumentCommand))] private string courtName;
        [ObservableProperty][NotifyCanExecuteChangedFor(nameof(CreateDocumentCommand))] private string courtCity;
        [ObservableProperty][NotifyCanExecuteChangedFor(nameof(CreateDocumentCommand))] private string plaintiffName;
        [ObservableProperty][NotifyCanExecuteChangedFor(nameof(CreateDocumentCommand))] private string plaintiffAddress;
        [ObservableProperty][NotifyCanExecuteChangedFor(nameof(CreateDocumentCommand))] private string plaintiffPhone;
        [ObservableProperty][NotifyCanExecuteChangedFor(nameof(CreateDocumentCommand))] private string defendantName;
        [ObservableProperty][NotifyCanExecuteChangedFor(nameof(CreateDocumentCommand))] private string defendantAddress;
        [ObservableProperty][NotifyCanExecuteChangedFor(nameof(CreateDocumentCommand))] private string defendantPhone;
        [ObservableProperty][NotifyCanExecuteChangedFor(nameof(CreateDocumentCommand))] private string claimLoanDate;
        [ObservableProperty][NotifyCanExecuteChangedFor(nameof(CreateDocumentCommand))] private string claimLoanAmount;
        [ObservableProperty][NotifyCanExecuteChangedFor(nameof(CreateDocumentCommand))] private string claimReturnDate;
        [ObservableProperty][NotifyCanExecuteChangedFor(nameof(CreateDocumentCommand))] private string claimDate = DateTime.Now.ToString("dd.MM.yyyy");

        // --- ПОЛЯ ДЛЯ ОТПУСКОВ ---
        [ObservableProperty][NotifyCanExecuteChangedFor(nameof(CreateDocumentCommand))] private string leaveStartDate;
        [ObservableProperty][NotifyCanExecuteChangedFor(nameof(CreateDocumentCommand))] private string leaveDaysCount;
        [ObservableProperty][NotifyCanExecuteChangedFor(nameof(CreateDocumentCommand))] private string leaveReason = "по семейным обстоятельствам";

        // --- ПОЛЯ ДЛЯ 3-НДФЛ ---
        [ObservableProperty][NotifyCanExecuteChangedFor(nameof(CreateDocumentCommand))] private string ndflLastName;
        [ObservableProperty][NotifyCanExecuteChangedFor(nameof(CreateDocumentCommand))] private string ndflFirstName;
        [ObservableProperty][NotifyCanExecuteChangedFor(nameof(CreateDocumentCommand))] private string ndflPatronymic;
        [ObservableProperty][NotifyCanExecuteChangedFor(nameof(CreateDocumentCommand))] private string ndflInn;
        [ObservableProperty][NotifyCanExecuteChangedFor(nameof(CreateDocumentCommand))] private string ndflPhone;
        [ObservableProperty][NotifyCanExecuteChangedFor(nameof(CreateDocumentCommand))] private string ndflTaxCode = "0100";
        [ObservableProperty][NotifyCanExecuteChangedFor(nameof(CreateDocumentCommand))] private string ndflYear = "2025";
        [ObservableProperty][NotifyCanExecuteChangedFor(nameof(CreateDocumentCommand))] private string ndflOktmo;
        [ObservableProperty][NotifyCanExecuteChangedFor(nameof(CreateDocumentCommand))] private string ndflEmployerName;
        [ObservableProperty][NotifyCanExecuteChangedFor(nameof(CreateDocumentCommand))] private string ndflEmployerInn;
        [ObservableProperty][NotifyCanExecuteChangedFor(nameof(CreateDocumentCommand))] private string ndflEmployerKpp;
        [ObservableProperty][NotifyCanExecuteChangedFor(nameof(CreateDocumentCommand))] private string ndflIncome;
        [ObservableProperty][NotifyCanExecuteChangedFor(nameof(CreateDocumentCommand))] private string ndflTaxWithheld;
        [ObservableProperty][NotifyCanExecuteChangedFor(nameof(CreateDocumentCommand))] private string ndflEduExp = "0";
        [ObservableProperty][NotifyCanExecuteChangedFor(nameof(CreateDocumentCommand))] private string ndflMedExp = "0";
        [ObservableProperty][NotifyCanExecuteChangedFor(nameof(CreateDocumentCommand))] private string ndflFitExp = "0";
        [ObservableProperty][NotifyCanExecuteChangedFor(nameof(CreateDocumentCommand))] private string ndflRefundAmount;
        [ObservableProperty][NotifyCanExecuteChangedFor(nameof(CreateDocumentCommand))] private string ndflBankBik;
        [ObservableProperty][NotifyCanExecuteChangedFor(nameof(CreateDocumentCommand))] private string ndflBankAccount;

        public DocumentEditorViewModel(GenerateDocumentUseCase generateUseCase, INameDeclensionService declensionService)
        {
            _generateUseCase = generateUseCase;
            _declensionService = declensionService;
        }

        [RelayCommand]
        private async Task AutoFillProfileAsync()
        {
            string lastName = Preferences.Default.Get("User_LastName", "");
            string firstName = Preferences.Default.Get("User_FirstName", "");
            string patronymic = Preferences.Default.Get("User_Patronymic", "");
            string fullName = $"{lastName} {firstName} {patronymic}".Trim();

            string phone = Preferences.Default.Get("User_Phone", "");
            string birthDate = Preferences.Default.Get("User_BirthDate", "");
            string address = Preferences.Default.Get("User_Address", "");

            string passport = "";
            string inn = "";

            try
            {
                passport = await SecureStorage.Default.GetAsync("User_Passport") ?? "";
                inn = await SecureStorage.Default.GetAsync("User_INN") ?? "";
            }
            catch (Exception) {}

            if (string.IsNullOrWhiteSpace(lastName))
            {
                await Shell.Current.DisplayAlertAsync("Внимание", "Сначала заполните Личные данные в разделе Профиль.", "ОК");
                return;
            }

            EmployeeName = fullName;
            NdflLastName = lastName;
            NdflFirstName = firstName;
            NdflPatronymic = patronymic;
            NdflInn = inn;
            NdflPhone = phone;

            PlaintiffName = fullName;
            PlaintiffAddress = address;
            PlaintiffPhone = phone;

            LenderName = fullName;
            LenderRequisites = $"Паспорт: {passport}\nАдрес: {address}\nИНН: {inn}";

            ReceiverName = fullName;
            ReceiverBirthDate = birthDate;
            ReceiverPassport = passport;
        }

        private bool CanCreateDocument() => DocumentType switch
        {
            DocTypes.Resignation => !string.IsNullOrWhiteSpace(DirectorName) && !string.IsNullOrWhiteSpace(EmployeeName) && !string.IsNullOrWhiteSpace(CompanyName),
            DocTypes.Loan => !string.IsNullOrWhiteSpace(LenderName) && !string.IsNullOrWhiteSpace(BorrowerName) && !string.IsNullOrWhiteSpace(LoanAmount) && !string.IsNullOrWhiteSpace(ReturnDate),
            DocTypes.Receipt => !string.IsNullOrWhiteSpace(ReceiverName) && !string.IsNullOrWhiteSpace(ReceiverPassport) && !string.IsNullOrWhiteSpace(SenderName) && !string.IsNullOrWhiteSpace(SenderPassport) && !string.IsNullOrWhiteSpace(ReceiptAmount),
            DocTypes.Claim => !string.IsNullOrWhiteSpace(CourtName) && !string.IsNullOrWhiteSpace(PlaintiffName) && !string.IsNullOrWhiteSpace(DefendantName) && !string.IsNullOrWhiteSpace(ClaimLoanAmount),
            DocTypes.PaidLeave => !string.IsNullOrWhiteSpace(DirectorName) && !string.IsNullOrWhiteSpace(EmployeeName) && !string.IsNullOrWhiteSpace(CompanyName) && !string.IsNullOrWhiteSpace(LeaveStartDate) && !string.IsNullOrWhiteSpace(LeaveDaysCount),
            DocTypes.UnpaidLeave => !string.IsNullOrWhiteSpace(DirectorName) && !string.IsNullOrWhiteSpace(EmployeeName) && !string.IsNullOrWhiteSpace(CompanyName) && !string.IsNullOrWhiteSpace(LeaveStartDate) && !string.IsNullOrWhiteSpace(LeaveDaysCount) && !string.IsNullOrWhiteSpace(LeaveReason),
            DocTypes.Ndfl3 => !string.IsNullOrWhiteSpace(NdflLastName) && !string.IsNullOrWhiteSpace(NdflInn) && !string.IsNullOrWhiteSpace(NdflIncome),
            _ => false
        };

        [RelayCommand(CanExecute = nameof(CanCreateDocument))]
        private async Task CreateDocumentAsync()
        {
            IsGenerating = true;
            try
            {
                var dataDictionary = DocumentType switch
                {
                    DocTypes.Resignation => GetResignationData(),
                    DocTypes.Loan => GetLoanData(),
                    DocTypes.Receipt => GetReceiptData(),
                    DocTypes.Claim => GetClaimData(),
                    DocTypes.PaidLeave => GetLeaveData(isPaid: true),
                    DocTypes.UnpaidLeave => GetLeaveData(isPaid: false),
                    DocTypes.Ndfl3 => GetNdfl3Data(),
                    _ => throw new InvalidOperationException($"Неизвестный тип документа: {DocumentType}")
                };

                dataDictionary.Add("Date", DateTime.Now.ToString("dd.MM.yyyy"));

                var request = new DocumentRequest(DocumentType, dataDictionary, new[] { ExportFormat.Pdf, ExportFormat.Xml });
                var results = await _generateUseCase.ExecuteAsync(request);

                if (results != null && results.Any())
                {
                    var navigationParams = new Dictionary<string, object> { { "GeneratedDocuments", results } };
                    await Shell.Current.GoToAsync(nameof(PdfPreviewPage), navigationParams);
                }
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlertAsync("Ошибка генерации", $"Не удалось сформировать документ: {ex.Message}", "ОК");
            }
            finally
            {
                IsGenerating = false;
            }
        }

        private Dictionary<string, string> GetResignationData() => new() { { "CompanyName", CompanyName }, { "DirectorName", _declensionService.DeclineName(DirectorName ?? "", NameCase.Dative) }, { "FullName", _declensionService.DeclineName(EmployeeName ?? "", NameCase.Genitive) } };
        private Dictionary<string, string> GetLoanData() => new() { { "City", City }, { "AgreementDate", AgreementDate }, { "LenderName", LenderName }, { "BorrowerName", BorrowerName }, { "TransferDate", TransferDate }, { "Amount", LoanAmount }, { "TransferMethod", TransferMethod }, { "ReturnDate", ReturnDate }, { "PenaltyRate", PenaltyRate }, { "LenderRequisites", LenderRequisites ?? "Реквизиты не указаны" }, { "BorrowerRequisites", BorrowerRequisites ?? "Реквизиты не указаны" } };
        private Dictionary<string, string> GetReceiptData() => new() { { "ReceiverName", ReceiverName }, { "ReceiverBirthDate", ReceiverBirthDate }, { "ReceiverPassport", ReceiverPassport }, { "SenderName", _declensionService.DeclineName(SenderName ?? "", NameCase.Genitive) }, { "SenderBirthDate", SenderBirthDate }, { "SenderPassport", SenderPassport }, { "Amount", ReceiptAmount }, { "ReturnDate", ReceiptReturnDate }, { "ReceiptDate", ReceiptDate } };
        private Dictionary<string, string> GetClaimData() => new() { { "CourtName", CourtName }, { "CourtCity", CourtCity }, { "PlaintiffName", PlaintiffName }, { "PlaintiffAddress", PlaintiffAddress }, { "PlaintiffPhone", PlaintiffPhone }, { "DefendantName", DefendantName }, { "DefendantAddress", DefendantAddress }, { "DefendantPhone", DefendantPhone }, { "LoanDate", ClaimLoanDate }, { "Amount", ClaimLoanAmount }, { "ReturnDate", ClaimReturnDate }, { "ClaimDate", ClaimDate }, { "DefendantGenitive", _declensionService.DeclineName(DefendantName ?? "", NameCase.Genitive) }, { "PlaintiffGenitive", _declensionService.DeclineName(PlaintiffName ?? "", NameCase.Genitive) } };
        private Dictionary<string, string> GetLeaveData(bool isPaid) { var dict = new Dictionary<string, string> { { "CompanyName", CompanyName }, { "DirectorName", _declensionService.DeclineName(DirectorName ?? "", NameCase.Dative) }, { "FullName", _declensionService.DeclineName(EmployeeName ?? "", NameCase.Genitive) }, { "RawEmployeeName", EmployeeName }, { "StartDate", LeaveStartDate }, { "DaysCount", LeaveDaysCount } }; if (!isPaid) dict.Add("Reason", LeaveReason); return dict; }
        private Dictionary<string, string> GetNdfl3Data() => new() { { "LastName", NdflLastName }, { "FirstName", NdflFirstName }, { "Patronymic", NdflPatronymic ?? "" }, { "INN", NdflInn }, { "Phone", NdflPhone }, { "TaxCode", NdflTaxCode }, { "Year", NdflYear }, { "OKTMO", NdflOktmo }, { "EmployerName", NdflEmployerName }, { "EmployerInn", NdflEmployerInn }, { "EmployerKpp", NdflEmployerKpp }, { "Income", NdflIncome }, { "TaxWithheld", NdflTaxWithheld }, { "RefundAmount", NdflRefundAmount }, { "EduExp", NdflEduExp }, { "MedExp", NdflMedExp }, { "FitExp", NdflFitExp }, { "BankBik", NdflBankBik }, { "BankAccount", NdflBankAccount } };
    }
}