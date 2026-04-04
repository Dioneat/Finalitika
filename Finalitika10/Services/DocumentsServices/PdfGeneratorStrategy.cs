using Finalitika10.Models;
using Finalitika10.Services.Interfaces.DocumentsService;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using iText.Layout.Properties;
using Border =  iText.Layout.Borders.Border;
using iText.IO.Font;
using iText.Kernel.Font;
using TextAlignment = iText.Layout.Properties.TextAlignment;
using Cell = iText.Layout.Element.Cell;

namespace Finalitika10.Services.DocumentsServices
{
    public class PdfGeneratorStrategy : IDocumentGeneratorStrategy
    {
        public ExportFormat Format => ExportFormat.Pdf;

        public async Task<DocumentResult> GenerateAsync(DocumentRequest request)
        {
            try
            {
                using var ms = new MemoryStream();
                var writer = new PdfWriter(ms);
                var pdf = new iText.Kernel.Pdf.PdfDocument(writer);
                var document = new Document(pdf);

                using var stream = await FileSystem.OpenAppPackageFileAsync("OpenSans-Regular.ttf");
                using var tempMs = new MemoryStream();
                await stream.CopyToAsync(tempMs);
                var fontBytes = tempMs.ToArray();

                var font = PdfFontFactory.CreateFont(fontBytes, PdfEncodings.IDENTITY_H);
                document.SetFont(font);

                switch (request.DocumentType)
                {
                    case "ResignationLetter":
                        BuildResignationLetter(document, request.Data);
                        break;
                    case "LoanAgreement":
                        BuildLoanAgreement(document, request.Data);
                        break;
                    case "MoneyReceipt":
                        BuildMoneyReceipt(document, request.Data);
                        break;
                    case "ClaimStatement":
                        BuildClaimStatement(document, request.Data);
                        break;
                    case "PaidLeaveApplication":
                        BuildPaidLeaveApplication(document, request.Data);
                        break;
                    case "UnpaidLeaveApplication":
                        BuildUnpaidLeaveApplication(document, request.Data);
                        break;
                    case "Ndfl3Declaration":
                        BuildNdfl3Pdf(document, request.Data); 
                        break;
                    default:
                        document.Add(new Paragraph("Неизвестный тип документа"));
                        break;
                }

                document.Close();

                return new DocumentResult(Format, ms.ToArray(), $"{request.DocumentType}.pdf");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"PDF Error: {ex}");
                throw;
            }
        }
        private void BuildNdfl3Pdf(Document document, Dictionary<string, string> data)
        {
            string lastName = data.GetValueOrDefault("LastName", "---");
            string firstName = data.GetValueOrDefault("FirstName", "---");
            string patronymic = data.GetValueOrDefault("Patronymic", "");
            string inn = data.GetValueOrDefault("INN", "---");
            string phone = data.GetValueOrDefault("Phone", "---");

            string taxCode = data.GetValueOrDefault("TaxCode", "---");
            string year = data.GetValueOrDefault("Year", "---");
            string oktmo = data.GetValueOrDefault("OKTMO", "---");

            string employer = data.GetValueOrDefault("EmployerName", "---");
            string empInn = data.GetValueOrDefault("EmployerInn", "---");
            string empKpp = data.GetValueOrDefault("EmployerKpp", "---");

            string income = data.GetValueOrDefault("Income", "0");
            string taxWithheld = data.GetValueOrDefault("TaxWithheld", "0");

            string edu = data.GetValueOrDefault("EduExp", "0");
            string med = data.GetValueOrDefault("MedExp", "0");
            string fit = data.GetValueOrDefault("FitExp", "0");
            string refund = data.GetValueOrDefault("RefundAmount", "0");

            string bankBik = data.GetValueOrDefault("BankBik", "---");
            string bankAcc = data.GetValueOrDefault("BankAccount", "---");
            string dateStr = DateTime.Now.ToString("dd.MM.yyyy");

            document.Add(new Paragraph("НАЛОГОВАЯ ДЕКЛАРАЦИЯ 3-НДФЛ\n(Сводная информация)")
                .SetTextAlignment(TextAlignment.CENTER).SetFontSize(14).SimulateBold());
            document.Add(new Paragraph($"Отчетный год: {year} | Код налогового органа: {taxCode}\n")
                .SetTextAlignment(TextAlignment.CENTER).SetFontSize(12));

            void AddSectionTitle(string title)
            {
                document.Add(new Paragraph(title)
                    .SimulateBold().SetBackgroundColor(iText.Kernel.Colors.ColorConstants.LIGHT_GRAY)
                    .SetPadding(3).SetMarginTop(10));
            }

            void AddTableRow(Table t, string label, string val)
            {
                t.AddCell(new Cell().Add(new Paragraph(label).SetFontSize(10)));
                t.AddCell(new Cell().Add(new Paragraph(val).SetFontSize(10).SimulateBold()));
            }

            AddSectionTitle("1. Сведения о налогоплательщике");
            Table table1 = new Table(UnitValue.CreatePercentArray(new float[] { 40, 60 })).UseAllAvailableWidth();
            AddTableRow(table1, "ФИО:", $"{lastName} {firstName} {patronymic}".Trim());
            AddTableRow(table1, "ИНН:", inn);
            AddTableRow(table1, "Контактный телефон:", phone);
            AddTableRow(table1, "ОКТМО:", oktmo);
            document.Add(table1);

            AddSectionTitle("2. Источники дохода");
            Table table2 = new Table(UnitValue.CreatePercentArray(new float[] { 40, 60 })).UseAllAvailableWidth();
            AddTableRow(table2, "Наименование источника (работодатель):", employer);
            AddTableRow(table2, "ИНН / КПП источника:", $"{empInn} / {empKpp}");
            AddTableRow(table2, "Сумма дохода (руб.):", income);
            AddTableRow(table2, "Сумма удержанного налога (руб.):", taxWithheld);
            document.Add(table2);

            AddSectionTitle("3. Заявленные социальные налоговые вычеты");
            Table table3 = new Table(UnitValue.CreatePercentArray(new float[] { 40, 60 })).UseAllAvailableWidth();
            AddTableRow(table3, "Расходы на обучение (руб.):", edu);
            AddTableRow(table3, "Расходы на лечение (руб.):", med);
            AddTableRow(table3, "Расходы на фитнес/спорт (руб.):", fit);

            decimal totalExp = (decimal.TryParse(edu, out var e) ? e : 0) +
                               (decimal.TryParse(med, out var m) ? m : 0) +
                               (decimal.TryParse(fit, out var f) ? f : 0);
            AddTableRow(table3, "Итого сумма расходов к вычету (руб.):", totalExp.ToString());
            document.Add(table3);

            AddSectionTitle("4. Возврат налога из бюджета");
            Table table4 = new Table(UnitValue.CreatePercentArray(new float[] { 40, 60 })).UseAllAvailableWidth();
            AddTableRow(table4, "Сумма налога, подлежащая возврату (руб.):", refund);
            AddTableRow(table4, "БИК банка для возврата:", bankBik);
            AddTableRow(table4, "Номер счета для возврата:", bankAcc);
            document.Add(table4);

            document.Add(new Paragraph("\nДостоверность и полноту сведений, указанных в настоящей декларации, подтверждаю.")
                .SetFontSize(10).SimulateItalic());

            Table signatureTable = new Table(UnitValue.CreatePercentArray(2)).UseAllAvailableWidth().SetMarginTop(30f);

            signatureTable.AddCell(new Cell().Add(new Paragraph($"Дата: {dateStr}"))
                .SetBorder(iText.Layout.Borders.Border.NO_BORDER).SetTextAlignment(TextAlignment.LEFT));

            string shortName = $"{lastName} {(firstName.Length > 0 ? firstName[0] + "." : "")} {(patronymic.Length > 0 ? patronymic[0] + "." : "")}";
            signatureTable.AddCell(new Cell().Add(new Paragraph($"Подпись: ______________ / {shortName} /"))
                .SetBorder(iText.Layout.Borders.Border.NO_BORDER).SetTextAlignment(TextAlignment.RIGHT));

            document.Add(signatureTable);
        }
        private void BuildPaidLeaveApplication(Document document, Dictionary<string, string> data)
        {
            string company = data.GetValueOrDefault("CompanyName", "---");
            string director = data.GetValueOrDefault("DirectorName", "---");
            string userGenitive = data.GetValueOrDefault("FullName", "---");
            string userRaw = data.GetValueOrDefault("RawEmployeeName", "---");

            string startDate = data.GetValueOrDefault("StartDate", "---");
            string daysCount = data.GetValueOrDefault("DaysCount", "---");
            string docDate = data.GetValueOrDefault("Date", DateTime.Now.ToString("dd.MM.yyyy"));

            document.Add(new Paragraph($"Директору {company}\n{director}\nот\n{userGenitive}")
                .SetTextAlignment(TextAlignment.RIGHT));

            document.Add(new Paragraph("\n\nЗАЯВЛЕНИЕ")
                .SetTextAlignment(TextAlignment.CENTER).SetFontSize(16).SimulateBold());

            document.Add(new Paragraph($"\nПрошу предоставить мне ежегодный оплачиваемый отпуск на {daysCount} календарных дней с {startDate} г.")
                .SetTextAlignment(TextAlignment.JUSTIFIED).SetFirstLineIndent(20f));

            Table signatureTable = new Table(UnitValue.CreatePercentArray(2)).UseAllAvailableWidth().SetMarginTop(40f);
            signatureTable.AddCell(new Cell().Add(new Paragraph(docDate))
                .SetBorder(iText.Layout.Borders.Border.NO_BORDER).SetTextAlignment(TextAlignment.LEFT));
            signatureTable.AddCell(new Cell().Add(new Paragraph($"______________ / {GetShortName(userRaw)} /"))
                .SetBorder(iText.Layout.Borders.Border.NO_BORDER).SetTextAlignment(TextAlignment.RIGHT));

            document.Add(signatureTable);
        }

        private void BuildUnpaidLeaveApplication(Document document, Dictionary<string, string> data)
        {
            string company = data.GetValueOrDefault("CompanyName", "---");
            string director = data.GetValueOrDefault("DirectorName", "---");
            string userGenitive = data.GetValueOrDefault("FullName", "---");
            string userRaw = data.GetValueOrDefault("RawEmployeeName", "---");

            string startDate = data.GetValueOrDefault("StartDate", "---");
            string daysCount = data.GetValueOrDefault("DaysCount", "---");
            string reason = data.GetValueOrDefault("Reason", "---");
            string docDate = data.GetValueOrDefault("Date", DateTime.Now.ToString("dd.MM.yyyy"));

            document.Add(new Paragraph($"Директору {company}\n{director}\nот\n{userGenitive}")
                .SetTextAlignment(TextAlignment.RIGHT));

            document.Add(new Paragraph("\n\nЗАЯВЛЕНИЕ")
                .SetTextAlignment(TextAlignment.CENTER).SetFontSize(16).SimulateBold());

            document.Add(new Paragraph($"\nПрошу предоставить мне отпуск без сохранения заработной платы на {daysCount} календарных дней с {startDate} г. {reason}.")
                .SetTextAlignment(TextAlignment.JUSTIFIED).SetFirstLineIndent(20f));

            Table signatureTable = new Table(UnitValue.CreatePercentArray(2)).UseAllAvailableWidth().SetMarginTop(40f);
            signatureTable.AddCell(new Cell().Add(new Paragraph(docDate))
                .SetBorder(iText.Layout.Borders.Border.NO_BORDER).SetTextAlignment(TextAlignment.LEFT));
            signatureTable.AddCell(new Cell().Add(new Paragraph($"______________ / {GetShortName(userRaw)} /"))
                .SetBorder(iText.Layout.Borders.Border.NO_BORDER).SetTextAlignment(TextAlignment.RIGHT));

            document.Add(signatureTable);
        }
        private void BuildClaimStatement(Document document, Dictionary<string, string> data)
        {
            string court = data.GetValueOrDefault("CourtName", "---");
            string city = data.GetValueOrDefault("CourtCity", "---");
            string plaintiff = data.GetValueOrDefault("PlaintiffName", "---");
            string pAddress = data.GetValueOrDefault("PlaintiffAddress", "---");
            string pPhone = data.GetValueOrDefault("PlaintiffPhone", "---");
            string defendant = data.GetValueOrDefault("DefendantName", "---");
            string dAddress = data.GetValueOrDefault("DefendantAddress", "---");
            string dPhone = data.GetValueOrDefault("DefendantPhone", "---");

            string loanDate = data.GetValueOrDefault("LoanDate", "---");
            string amount = data.GetValueOrDefault("Amount", "---");
            string returnDate = data.GetValueOrDefault("ReturnDate", "---");
            string claimDate = data.GetValueOrDefault("ClaimDate", DateTime.Now.ToString("dd.MM.yyyy"));

            string pGenitive = data.GetValueOrDefault("PlaintiffGenitive", plaintiff);
            string dGenitive = data.GetValueOrDefault("DefendantGenitive", defendant);

            Table headerTable = new Table(UnitValue.CreatePercentArray(new float[] { 50, 50 })).UseAllAvailableWidth();

            headerTable.AddCell(new Cell().SetBorder(iText.Layout.Borders.Border.NO_BORDER));

            Cell headerCell = new Cell().SetBorder(iText.Layout.Borders.Border.NO_BORDER);
            headerCell.Add(new Paragraph($"В {court}").SimulateBold());
            headerCell.Add(new Paragraph($"{city}\n\n"));

            headerCell.Add(new Paragraph("Истец:").SimulateBold());
            headerCell.Add(new Paragraph($"{plaintiff}\nАдрес: {pAddress}\nТелефон: {pPhone}\n\n"));

            headerCell.Add(new Paragraph("Ответчик:").SimulateBold());
            headerCell.Add(new Paragraph($"{defendant}\nАдрес: {dAddress}\nТелефон: {dPhone}"));

            headerTable.AddCell(headerCell);
            document.Add(headerTable);

            document.Add(new Paragraph("\nИСКОВОЕ ЗАЯВЛЕНИЕ")
                .SetTextAlignment(TextAlignment.CENTER).SetFontSize(14).SimulateBold());
            document.Add(new Paragraph("о взыскании долга по договору займа")
                .SetTextAlignment(TextAlignment.CENTER).SetFontSize(12).SimulateBold());

            Paragraph p1 = new Paragraph($"\n{loanDate} г. между истцом и ответчиком был заключен договор займа, согласно которому истец передал ответчику денежную сумму в размере {amount}. В подтверждение договора составлена расписка (копия прилагается).")
                .SetTextAlignment(TextAlignment.JUSTIFIED).SetFirstLineIndent(20f);
            document.Add(p1);

            Paragraph p2 = new Paragraph($"В соответствии с вышеуказанным договором ответчик обязан возвратить истцу деньги {returnDate} г. В указанный срок долг ответчиком возвращен не был.")
                .SetTextAlignment(TextAlignment.JUSTIFIED).SetFirstLineIndent(20f);
            document.Add(p2);

            Paragraph p3 = new Paragraph("На предложение истца о добровольной уплате долга ответчик ответил отказом (или проигнорировал требование).")
                .SetTextAlignment(TextAlignment.JUSTIFIED).SetFirstLineIndent(20f);
            document.Add(p3);

            Paragraph p4 = new Paragraph("На основании вышеизложенного, руководствуясь ст. ст. 807 - 808, ст. 810 Гражданского кодекса Российской Федерации, ст. ст. 131, 132 Гражданского процессуального кодекса Российской Федерации,")
                .SetTextAlignment(TextAlignment.JUSTIFIED).SetFirstLineIndent(20f);
            document.Add(p4);

            document.Add(new Paragraph("\nПРОШУ:").SimulateBold().SetTextAlignment(TextAlignment.CENTER));

            document.Add(new Paragraph($"Взыскать с {dGenitive} в пользу {pGenitive} долг по договору займа в сумме {amount}.")
                .SetTextAlignment(TextAlignment.JUSTIFIED).SetFirstLineIndent(20f));

            document.Add(new Paragraph("\nПриложения:").SimulateBold());
            document.Add(new Paragraph("1. Копия искового заявления для ответчика."));
            document.Add(new Paragraph("2. Документ об оплате госпошлины."));
            document.Add(new Paragraph("3. Копия договора займа и/или расписки."));

            Table signatureTable = new Table(UnitValue.CreatePercentArray(2)).UseAllAvailableWidth().SetMarginTop(30f);
            signatureTable.AddCell(new Cell().Add(new Paragraph($"{claimDate} г."))
                .SetBorder(iText.Layout.Borders.Border.NO_BORDER).SetTextAlignment(TextAlignment.LEFT));

            signatureTable.AddCell(new Cell().Add(new Paragraph($"________________ / {GetShortName(plaintiff)} /"))
                .SetBorder(iText.Layout.Borders.Border.NO_BORDER).SetTextAlignment(TextAlignment.RIGHT));

            document.Add(signatureTable);
        }
        private void BuildMoneyReceipt(Document document, Dictionary<string, string> data)
        {
            string receiver = data.GetValueOrDefault("ReceiverName", "---");
            string receiverBirth = data.GetValueOrDefault("ReceiverBirthDate", "---");
            string receiverPass = data.GetValueOrDefault("ReceiverPassport", "---");

            string sender = data.GetValueOrDefault("SenderName", "---");
            string senderBirth = data.GetValueOrDefault("SenderBirthDate", "---");
            string senderPass = data.GetValueOrDefault("SenderPassport", "---");

            string amount = data.GetValueOrDefault("Amount", "---");
            string returnDate = data.GetValueOrDefault("ReturnDate", "---");
            string receiptDate = data.GetValueOrDefault("ReceiptDate", DateTime.Now.ToString("dd.MM.yyyy"));

            document.Add(new Paragraph("Расписка")
                .SetTextAlignment(TextAlignment.CENTER).SetFontSize(14).SimulateBold());

            string receiptText = $"Я, {receiver}, {receiverBirth} года рождения, паспорт {receiverPass}, " +
                                 $"получил от {sender}, {senderBirth} года рождения, паспорт {senderPass}, " +
                                 $"наличные деньги — {amount} руб. — на хранение до {returnDate}.";

            document.Add(new Paragraph($"\n{receiptText}")
                .SetTextAlignment(TextAlignment.JUSTIFIED)
                .SetFirstLineIndent(20f)); 
            string receiverLastName = receiver.Split(' ').FirstOrDefault() ?? "";

            Table signatureTable = new Table(UnitValue.CreatePercentArray(2)).UseAllAvailableWidth().SetMarginTop(30f);

            signatureTable.AddCell(new Cell().Add(new Paragraph(receiverLastName))
                .SetBorder(iText.Layout.Borders.Border.NO_BORDER).SetTextAlignment(TextAlignment.LEFT));

            signatureTable.AddCell(new Cell().Add(new Paragraph(receiptDate))
                .SetBorder(iText.Layout.Borders.Border.NO_BORDER).SetTextAlignment(TextAlignment.RIGHT));

            document.Add(signatureTable);
        }

        private void BuildResignationLetter(Document document, Dictionary<string, string> data)
        {
            string company = data.GetValueOrDefault("CompanyName", "---");
            string director = data.GetValueOrDefault("DirectorName", "---");
            string user = data.GetValueOrDefault("FullName", "---");
            string date = data.GetValueOrDefault("Date", DateTime.Now.ToString("dd.MM.yyyy"));

            document.Add(new Paragraph($"Директору {company}\n{director}\nот\n{user}")
                .SetTextAlignment(TextAlignment.RIGHT));

            document.Add(new Paragraph("\n\nЗАЯВЛЕНИЕ").SetTextAlignment(TextAlignment.CENTER).SetFontSize(16));
            document.Add(new Paragraph($"\nПрошу уволить меня по собственному желанию {date}."));
        }

        private void BuildLoanAgreement(Document document, Dictionary<string, string> data)
        {
            string city = data.GetValueOrDefault("City", "г. Москва");
            string agreementDate = data.GetValueOrDefault("AgreementDate", DateTime.Now.ToString("dd.MM.yyyy"));
            string lender = data.GetValueOrDefault("LenderName", "---");
            string borrower = data.GetValueOrDefault("BorrowerName", "---");
            string transferDate = data.GetValueOrDefault("TransferDate", "---");
            string amount = data.GetValueOrDefault("Amount", "---");
            string transferMethod = data.GetValueOrDefault("TransferMethod", "наличными");
            string returnDate = data.GetValueOrDefault("ReturnDate", "---");
            string penalty = data.GetValueOrDefault("PenaltyRate", "1%");

            string lenderReqs = data.GetValueOrDefault("LenderRequisites", "");
            string borrowerReqs = data.GetValueOrDefault("BorrowerRequisites", "");

            document.Add(new Paragraph("Договор беспроцентного займа")
                .SetTextAlignment(TextAlignment.CENTER).SetFontSize(14).SimulateBold());

            Table headerTable = new Table(UnitValue.CreatePercentArray(2)).UseAllAvailableWidth();
            headerTable.AddCell(new Cell().Add(new Paragraph(city)).SetBorder(Border.NO_BORDER).SetTextAlignment(TextAlignment.LEFT));
            headerTable.AddCell(new Cell().Add(new Paragraph(agreementDate)).SetBorder(Border.NO_BORDER).SetTextAlignment(TextAlignment.RIGHT));
            document.Add(headerTable);

            document.Add(new Paragraph($"\n{lender} (далее — Заимодавец) с одной стороны и {borrower} (далее — Заемщик) с другой стороны, совместно именуемые «Стороны», заключили Договор о нижеследующем.\n")
                .SetTextAlignment(TextAlignment.JUSTIFIED));

            document.Add(new Paragraph("1. Предмет Договора").SimulateBold());
            document.Add(new Paragraph($"1.1. Заимодавец до {transferDate} передает в собственность Заемщику {amount} (далее — Сумма займа), а Заемщик обязуется вернуть Заимодавцу Сумму займа в порядке и в сроки, установленные Договором."));
            document.Add(new Paragraph("1.2. Заемщик не выплачивает Заимодавцу проценты за пользование Суммой займа — заем беспроцентный."));
            document.Add(new Paragraph($"1.3. Заимодавец передает Сумму займа Заемщику {transferMethod}. Заемщик, получив деньги, выдает Заимодавцу расписку об этом.\n"));

            document.Add(new Paragraph("2. Срок действия Договора").SimulateBold());
            document.Add(new Paragraph("2.1. Договор вступает в силу со дня его подписания обеими сторонами и действует до полного выполнения ими обязательств по Договором."));
            document.Add(new Paragraph("2.2. Любая из Сторон вправе в одностороннем порядке расторгнуть Договор по письменному требованию. Об этом нужно письменно уведомить другую Сторону минимум за один месяц до даты расторжения Договора."));
            document.Add(new Paragraph("2.3. При одностороннем отказе от исполнения Договора Сторона, которая заявила об этом, возмещает другой Стороне убытки, вызванные расторжением.\n"));

            document.Add(new Paragraph("3. Порядок расчетов").SimulateBold());
            document.Add(new Paragraph($"3.1. Заемщик возвращает полную Сумму займа Заимодавцу до {returnDate}. Получив деньги, Заимодавец возвращает Заемщику расписку, указанную в п. 1.3 Договора."));
            document.Add(new Paragraph("3.2. Сумма займа может быть возвращена досрочно полностью или по частям.\n"));

            document.Add(new Paragraph("4. Ответственность Сторон").SimulateBold());
            document.Add(new Paragraph($"4.1. Если Заемщик нарушает срок возврата Суммы займа, Заимодавец вправе потребовать уплаты пени — {penalty} от невозвращенной в срок суммы за каждый день просрочки."));
            document.Add(new Paragraph("4.2. Уплата пени не освобождает Заемщика от возврата Суммы займа.\n"));

            document.Add(new Paragraph("5. Форс-мажор").SimulateBold());
            document.Add(new Paragraph("5.1. Сторона, которая подверглась действию обстоятельств непреодолимой силы, должна в течение 5 календарных дней уведомить другую сторону об этом."));
            document.Add(new Paragraph("5.2. Если обязательства не были выполнены из-за форс-мажора более месяца, Стороны проводят переговоры."));
            document.Add(new Paragraph("5.3. Стороны признают, что неплатежеспособность — не обстоятельство непреодолимой силы.\n"));

            document.Add(new Paragraph("6. Разрешение споров").SimulateBold());
            document.Add(new Paragraph("6.1. Споры по вопросам исполнения Договора разрешаются на переговорах."));
            document.Add(new Paragraph("6.2. Если согласие не достигнуто, спор передается в суд.\n"));

            document.Add(new Paragraph("7. Конфиденциальность").SimulateBold());
            document.Add(new Paragraph("7.1. Условия Договора конфиденциальны и не подлежат разглашению.\n"));

            document.Add(new Paragraph("8. Прочие условия").SimulateBold());
            document.Add(new Paragraph("8.1. Изменения к Договору действительны, если зафиксированы на бумаге и подписаны Сторонами."));
            document.Add(new Paragraph("8.2. Договор составлен в двух экземплярах равной юридической силы.\n"));

            document.Add(new Paragraph("9. Адреса и реквизиты Сторон").SimulateBold());

            Table requisitesTable = new Table(UnitValue.CreatePercentArray(2)).UseAllAvailableWidth();

            Cell lenderCell = new Cell().SetBorder(iText.Layout.Borders.Border.NO_BORDER);
            lenderCell.Add(new Paragraph("Заимодавец").SimulateBold());
            lenderCell.Add(new Paragraph(lender).SimulateBold());
            lenderCell.Add(new Paragraph(lenderReqs).SetFontSize(10));
            lenderCell.Add(new Paragraph($"\n______________ / {GetShortName(lender)} /"));

            Cell borrowerCell = new Cell().SetBorder(iText.Layout.Borders.Border.NO_BORDER);
            borrowerCell.Add(new Paragraph("Заемщик").SimulateBold());
            borrowerCell.Add(new Paragraph(borrower).SimulateBold());
            borrowerCell.Add(new Paragraph(borrowerReqs).SetFontSize(10));
            borrowerCell.Add(new Paragraph($"\n______________ / {GetShortName(borrower)} /"));

            requisitesTable.AddCell(lenderCell);
            requisitesTable.AddCell(borrowerCell);

            document.Add(requisitesTable);
        }

        private string GetShortName(string fullName)
        {
            if (string.IsNullOrWhiteSpace(fullName)) return "";
            var parts = fullName.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 3)
                return $"{parts[0]} {parts[1][0]}. {parts[2][0]}.";
            return fullName;
        }
    }
}
