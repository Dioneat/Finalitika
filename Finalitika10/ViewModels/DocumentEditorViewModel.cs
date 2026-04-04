using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Finalitika10.Models;
using Finalitika10.Services.Interfaces.DocumentsService;
using Finalitika10.Views;

namespace Finalitika10.ViewModels
{
    [QueryProperty(nameof(DocumentType), "DocType")]
    public partial class DocumentEditorViewModel : ObservableObject
    {
        private readonly GenerateDocumentUseCase _generateUseCase;
        private readonly INameDeclensionService _declensionService;


        public bool IsResignation => DocumentType == "ResignationLetter";
        public bool IsLoan => DocumentType == "LoanAgreement";
        public bool IsReceipt => DocumentType == "MoneyReceipt";
        public bool IsClaim => DocumentType == "ClaimStatement";
        public bool IsPaidLeave => DocumentType == "PaidLeaveApplication";
        public bool IsUnpaidLeave => DocumentType == "UnpaidLeaveApplication";
        public bool IsNdfl3 => DocumentType == "Ndfl3Declaration";

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(IsResignation))]
        [NotifyPropertyChangedFor(nameof(IsLoan))]
        [NotifyPropertyChangedFor(nameof(IsReceipt))]
        [NotifyPropertyChangedFor(nameof(IsClaim))]
        [NotifyPropertyChangedFor(nameof(IsNdfl3))]
        [NotifyPropertyChangedFor(nameof(IsPaidLeave))]   
        [NotifyPropertyChangedFor(nameof(IsUnpaidLeave))] 
        private string documentType;

        [ObservableProperty] private bool isGenerating;

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
        [ObservableProperty][NotifyCanExecuteChangedFor(nameof(CreateDocumentCommand))] private string transferMethod = "наличными"; // По умолчанию
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
        [ObservableProperty][NotifyCanExecuteChangedFor(nameof(CreateDocumentCommand))] private string leaveReason = "по семейным обстоятельствам"; // Для отпуска за свой счет

        // --- ПОЛЯ ДЛЯ 3-НДФЛ ---
        // Налогоплательщик
        [ObservableProperty][NotifyCanExecuteChangedFor(nameof(CreateDocumentCommand))] private string ndflLastName;
        [ObservableProperty][NotifyCanExecuteChangedFor(nameof(CreateDocumentCommand))] private string ndflFirstName;
        [ObservableProperty][NotifyCanExecuteChangedFor(nameof(CreateDocumentCommand))] private string ndflPatronymic;
        [ObservableProperty][NotifyCanExecuteChangedFor(nameof(CreateDocumentCommand))] private string ndflInn;
        [ObservableProperty][NotifyCanExecuteChangedFor(nameof(CreateDocumentCommand))] private string ndflPhone;

        // Налоговая и год
        [ObservableProperty][NotifyCanExecuteChangedFor(nameof(CreateDocumentCommand))] private string ndflTaxCode = "0100"; // Код инспекции
        [ObservableProperty][NotifyCanExecuteChangedFor(nameof(CreateDocumentCommand))] private string ndflYear = "2025";
        [ObservableProperty][NotifyCanExecuteChangedFor(nameof(CreateDocumentCommand))] private string ndflOktmo;

        // Доходы и налоги (Работодатель/Брокер)
        [ObservableProperty][NotifyCanExecuteChangedFor(nameof(CreateDocumentCommand))] private string ndflEmployerName;
        [ObservableProperty][NotifyCanExecuteChangedFor(nameof(CreateDocumentCommand))] private string ndflEmployerInn;
        [ObservableProperty][NotifyCanExecuteChangedFor(nameof(CreateDocumentCommand))] private string ndflEmployerKpp;
        [ObservableProperty][NotifyCanExecuteChangedFor(nameof(CreateDocumentCommand))] private string ndflIncome; // Доход
        [ObservableProperty][NotifyCanExecuteChangedFor(nameof(CreateDocumentCommand))] private string ndflTaxWithheld; // Удержанный налог

        // Вычеты и возврат
        [ObservableProperty][NotifyCanExecuteChangedFor(nameof(CreateDocumentCommand))] private string ndflEduExp = "0"; // Обучение
        [ObservableProperty][NotifyCanExecuteChangedFor(nameof(CreateDocumentCommand))] private string ndflMedExp = "0"; // Лечение
        [ObservableProperty][NotifyCanExecuteChangedFor(nameof(CreateDocumentCommand))] private string ndflFitExp = "0"; // Фитнес
        [ObservableProperty][NotifyCanExecuteChangedFor(nameof(CreateDocumentCommand))] private string ndflRefundAmount; // К возврату

        // Реквизиты для возврата
        [ObservableProperty][NotifyCanExecuteChangedFor(nameof(CreateDocumentCommand))] private string ndflBankBik;
        [ObservableProperty][NotifyCanExecuteChangedFor(nameof(CreateDocumentCommand))] private string ndflBankAccount;

        public DocumentEditorViewModel(GenerateDocumentUseCase generateUseCase, INameDeclensionService declensionService)
        {
            _generateUseCase = generateUseCase;
            _declensionService = declensionService;
        }

        private bool CanCreateDocument() => DocumentType switch
        {
            "ResignationLetter" => !string.IsNullOrWhiteSpace(DirectorName) && !string.IsNullOrWhiteSpace(EmployeeName) && !string.IsNullOrWhiteSpace(CompanyName),
            "LoanAgreement" => !string.IsNullOrWhiteSpace(LenderName) && !string.IsNullOrWhiteSpace(BorrowerName) && !string.IsNullOrWhiteSpace(LoanAmount) && !string.IsNullOrWhiteSpace(ReturnDate),
            "MoneyReceipt" => !string.IsNullOrWhiteSpace(ReceiverName) && !string.IsNullOrWhiteSpace(ReceiverPassport) && !string.IsNullOrWhiteSpace(SenderName) && !string.IsNullOrWhiteSpace(SenderPassport) && !string.IsNullOrWhiteSpace(ReceiptAmount),
            "ClaimStatement" => !string.IsNullOrWhiteSpace(CourtName) && !string.IsNullOrWhiteSpace(PlaintiffName) && !string.IsNullOrWhiteSpace(DefendantName) && !string.IsNullOrWhiteSpace(ClaimLoanAmount),
            "PaidLeaveApplication" => !string.IsNullOrWhiteSpace(DirectorName) && !string.IsNullOrWhiteSpace(EmployeeName) && !string.IsNullOrWhiteSpace(CompanyName) && !string.IsNullOrWhiteSpace(LeaveStartDate) && !string.IsNullOrWhiteSpace(LeaveDaysCount),
            "UnpaidLeaveApplication" => !string.IsNullOrWhiteSpace(DirectorName) && !string.IsNullOrWhiteSpace(EmployeeName) && !string.IsNullOrWhiteSpace(CompanyName) && !string.IsNullOrWhiteSpace(LeaveStartDate) && !string.IsNullOrWhiteSpace(LeaveDaysCount) && !string.IsNullOrWhiteSpace(LeaveReason),
            "Ndfl3Declaration" => !string.IsNullOrWhiteSpace(NdflLastName) && !string.IsNullOrWhiteSpace(NdflInn) && !string.IsNullOrWhiteSpace(NdflIncome),
            _ => false
        };

        [RelayCommand(CanExecute = nameof(CanCreateDocument))]
        private async Task CreateDocumentAsync()
        {
            IsGenerating = true;
            try
            {
                // 1. Вызываем нужный метод сборки данных через switch
                var dataDictionary = DocumentType switch
                {
                    "ResignationLetter" => GetResignationData(),
                    "LoanAgreement" => GetLoanData(),
                    "MoneyReceipt" => GetReceiptData(),
                    "ClaimStatement" => GetClaimData(),
                    "PaidLeaveApplication" => GetLeaveData(isPaid: true),
                    "UnpaidLeaveApplication" => GetLeaveData(isPaid: false),
                    "Ndfl3Declaration" => GetNdfl3Data(),
                    _ => throw new InvalidOperationException($"Неизвестный тип документа: {DocumentType}")
                };

                // 2. Добавляем общие поля
                dataDictionary.Add("Date", DateTime.Now.ToString("dd.MM.yyyy"));

                // 3. Отправляем в UseCase
                var request = new DocumentRequest(DocumentType, dataDictionary, new[] { ExportFormat.Pdf, ExportFormat.Xml });
                var results = await _generateUseCase.ExecuteAsync(request);

                if (results != null && results.Any())
                {
                    var navigationParams = new Dictionary<string, object> { { "GeneratedDocuments", results } };
                    await Shell.Current.GoToAsync(nameof(PdfPreviewPage), navigationParams);
                }
            }
            finally
            {
                IsGenerating = false;
            }
        }
        // --- МЕТОДЫ СБОРКИ ДАННЫХ ---

        private Dictionary<string, string> GetResignationData() => new()
        {
            { "CompanyName", CompanyName },
            { "DirectorName", _declensionService.DeclineName(DirectorName, NameCase.Dative) },
            { "FullName", _declensionService.DeclineName(EmployeeName, NameCase.Genitive) }
        };

        private Dictionary<string, string> GetLoanData() => new()
        {
            { "City", City }, { "AgreementDate", AgreementDate },
            { "LenderName", LenderName }, { "BorrowerName", BorrowerName },
            { "TransferDate", TransferDate }, { "Amount", LoanAmount },
            { "TransferMethod", TransferMethod }, { "ReturnDate", ReturnDate },
            { "PenaltyRate", PenaltyRate },
            { "LenderRequisites", LenderRequisites ?? "Реквизиты не указаны" },
            { "BorrowerRequisites", BorrowerRequisites ?? "Реквизиты не указаны" }
        };

        private Dictionary<string, string> GetReceiptData() => new()
        {
            { "ReceiverName", ReceiverName }, { "ReceiverBirthDate", ReceiverBirthDate }, { "ReceiverPassport", ReceiverPassport },
            { "SenderName", _declensionService.DeclineName(SenderName, NameCase.Genitive) },
            { "SenderBirthDate", SenderBirthDate }, { "SenderPassport", SenderPassport },
            { "Amount", ReceiptAmount }, { "ReturnDate", ReceiptReturnDate }, { "ReceiptDate", ReceiptDate }
        };

        private Dictionary<string, string> GetClaimData() => new()
        {
            { "CourtName", CourtName }, { "CourtCity", CourtCity },
            { "PlaintiffName", PlaintiffName }, { "PlaintiffAddress", PlaintiffAddress }, { "PlaintiffPhone", PlaintiffPhone },
            { "DefendantName", DefendantName }, { "DefendantAddress", DefendantAddress }, { "DefendantPhone", DefendantPhone },
            { "LoanDate", ClaimLoanDate }, { "Amount", ClaimLoanAmount },
            { "ReturnDate", ClaimReturnDate }, { "ClaimDate", ClaimDate },
            { "DefendantGenitive", _declensionService.DeclineName(DefendantName, NameCase.Genitive) },
            { "PlaintiffGenitive", _declensionService.DeclineName(PlaintiffName, NameCase.Genitive) }
        };

        private Dictionary<string, string> GetLeaveData(bool isPaid)
        {
            var dict = new Dictionary<string, string>
            {   
                { "CompanyName", CompanyName },
                { "DirectorName", _declensionService.DeclineName(DirectorName, NameCase.Dative) },
                { "FullName", _declensionService.DeclineName(EmployeeName, NameCase.Genitive) },
                { "RawEmployeeName", EmployeeName },
                { "StartDate", LeaveStartDate },
                { "DaysCount", LeaveDaysCount }
            };
            if (!isPaid) dict.Add("Reason", LeaveReason);
            return dict;
        }

        private Dictionary<string, string> GetNdfl3Data() => new()
        {
           // Данные налогоплательщика
            { "LastName", NdflLastName },
            { "FirstName", NdflFirstName },
            { "Patronymic", NdflPatronymic ?? "" },
            { "INN", NdflInn },
            { "Phone", NdflPhone },
    
            // Налоговая и год
            { "TaxCode", NdflTaxCode },
            { "Year", NdflYear },
            { "OKTMO", NdflOktmo },

            // Источник дохода (Работодатель)
            { "EmployerName", NdflEmployerName },
            { "EmployerInn", NdflEmployerInn },
            { "EmployerKpp", NdflEmployerKpp },

            // Суммы дохода и налога
            { "Income", NdflIncome },
            { "TaxWithheld", NdflTaxWithheld },
            { "RefundAmount", NdflRefundAmount },

            // Социальные вычеты (расходы)
            { "EduExp", NdflEduExp },
            { "MedExp", NdflMedExp },
            { "FitExp", NdflFitExp },

            // Реквизиты для возврата средств
            { "BankBik", NdflBankBik },
            { "BankAccount", NdflBankAccount }
                };
    }
}
