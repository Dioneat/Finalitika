using ClosedXML.Excel;
using CommunityToolkit.Maui.Storage;
using Finalitika10.Models;
using System.Globalization;

namespace Finalitika10.Services.PlanServices
{
    public sealed class ProjectExcelExportService : IProjectExcelExportService
    {
        public async Task<ProjectExcelExportResult> ExportAsync(
            FinancialProject project,
            CancellationToken cancellationToken = default)
        {
            if (project is null)
            {
                return new ProjectExcelExportResult
                {
                    IsSuccess = false,
                    Message = "Проект не найден."
                };
            }

            using var workbook = new XLWorkbook();

            BuildSummarySheet(workbook, project);
            BuildParticipantsSheet(workbook, project);
            BuildContributionsSheet(workbook, project);

            if (project.Expenses.Count > 0)
            {
                BuildExpensesSheet(workbook, project);
            }

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            stream.Position = 0;

            string safeTitle = MakeSafeFileName(project.Title);
            string fileName = $"Проект_{safeTitle}_{DateTime.Now:yyyyMMdd_HHmm}.xlsx";

            var result = await FileSaver.Default.SaveAsync(
                fileName,
                stream,
                cancellationToken);

            if (!result.IsSuccessful)
            {
                return new ProjectExcelExportResult
                {
                    IsSuccess = false,
                    Message = result.Exception?.Message ?? "Не удалось сохранить Excel-файл."
                };
            }

            return new ProjectExcelExportResult
            {
                IsSuccess = true,
                Message = "Excel-файл успешно сохранён.",
                FilePath = result.FilePath
            };
        }

        private static void BuildSummarySheet(XLWorkbook workbook, FinancialProject project)
        {
            var ws = workbook.Worksheets.Add("Проект");

            ws.Cell("A1").Value = "Финалитика — выгрузка проекта";
            ws.Range("A1:B1").Merge();
            ws.Cell("A1").Style.Font.Bold = true;
            ws.Cell("A1").Style.Font.FontSize = 16;
            ws.Cell("A1").Style.Fill.BackgroundColor = XLColor.FromHtml("#D9EAF7");

            ws.Cell("A3").Value = "Название";
            ws.Cell("B3").Value = project.Title;

            ws.Cell("A4").Value = "Тип проекта";
            ws.Cell("B4").Value = GetProjectTypeText(project.ProjectType);

            ws.Cell("A5").Value = "Тип расчёта";
            ws.Cell("B5").Value = GetCalculationTypeText(project.TargetCalculationType);

            ws.Cell("A6").Value = "Цель";
            ws.Cell("B6").Value = project.TargetAmount;

            ws.Cell("A7").Value = "Собрано";
            ws.Cell("B7").Value = project.CollectedAmount;

            ws.Cell("A8").Value = "Прогресс";
            ws.Cell("B8").Value = project.TargetAmount > 0
                ? project.CollectedAmount / project.TargetAmount
                : 0m;

            ws.Cell("A9").Value = "Участников";
            ws.Cell("B9").Value = project.Participants.Count;

            ws.Cell("A10").Value = "Расходов";
            ws.Cell("B10").Value = project.Expenses.Count;

            var labels = ws.Range("A3:A10");
            labels.Style.Font.Bold = true;
            labels.Style.Fill.BackgroundColor = XLColor.FromHtml("#F4F6F8");

            ws.Cell("B6").Style.NumberFormat.Format = "#,##0.00 ₽";
            ws.Cell("B7").Style.NumberFormat.Format = "#,##0.00 ₽";
            ws.Cell("B8").Style.NumberFormat.Format = "0.00%";

            ws.Columns().AdjustToContents();

            ws.Column("A").Width = Math.Max(ws.Column("A").Width, 20);
            ws.Column("B").Width = Math.Max(ws.Column("B").Width, 24);
        }

        private static void BuildParticipantsSheet(XLWorkbook workbook, FinancialProject project)
        {
            var ws = workbook.Worksheets.Add("Участники");

            ws.Cell("A1").Value = "Имя";
            ws.Cell("B1").Value = "Внесено";
            ws.Cell("C1").Value = "Доля";
            ws.Cell("D1").Value = "Баланс";
            ws.Cell("E1").Value = "Статус";

            StyleHeader(ws.Range("A1:E1"));

            int row = 2;

            foreach (var participant in project.Participants)
            {
                ws.Cell(row, 1).Value = participant.Name;
                ws.Cell(row, 2).Value = participant.Contributed;
                ws.Cell(row, 3).Value = participant.RequiredShare;
                ws.Cell(row, 4).Value = participant.Balance;
                ws.Cell(row, 5).Value = participant.StatusText;
                row++;
            }

            ws.Column(2).Style.NumberFormat.Format = "#,##0.00 ₽";
            ws.Column(3).Style.NumberFormat.Format = "#,##0.00 ₽";
            ws.Column(4).Style.NumberFormat.Format = "#,##0.00 ₽";

            ws.Columns().AdjustToContents();
            ws.SheetView.FreezeRows(1);
        }

        private static void BuildContributionsSheet(XLWorkbook workbook, FinancialProject project)
        {
            var ws = workbook.Worksheets.Add("Взносы");

            ws.Cell("A1").Value = "Участник";
            ws.Cell("B1").Value = "Дата";
            ws.Cell("C1").Value = "Сумма";
            ws.Cell("D1").Value = "Комментарий";

            StyleHeader(ws.Range("A1:D1"));

            int row = 2;

            foreach (var participant in project.Participants)
            {
                foreach (var contribution in participant.Contributions.OrderBy(x => x.Date))
                {
                    ws.Cell(row, 1).Value = participant.Name;
                    ws.Cell(row, 2).Value = contribution.Date;
                    ws.Cell(row, 3).Value = contribution.Amount;
                    ws.Cell(row, 4).Value = contribution.Comment;
                    row++;
                }
            }

            ws.Column(2).Style.DateFormat.Format = "dd.MM.yyyy HH:mm";
            ws.Column(3).Style.NumberFormat.Format = "#,##0.00 ₽";

            ws.Columns().AdjustToContents();
            ws.SheetView.FreezeRows(1);
        }

        private static void BuildExpensesSheet(XLWorkbook workbook, FinancialProject project)
        {
            var ws = workbook.Worksheets.Add("Расходы");

            ws.Cell("A1").Value = "Наименование";
            ws.Cell("B1").Value = "Сумма";

            StyleHeader(ws.Range("A1:B1"));

            int row = 2;

            foreach (var expense in project.Expenses)
            {
                ws.Cell(row, 1).Value = expense.Title;
                ws.Cell(row, 2).Value = expense.Amount;
                row++;
            }

            ws.Cell(row, 1).Value = "Итого";
            ws.Cell(row, 1).Style.Font.Bold = true;
            ws.Cell(row, 2).FormulaA1 = $"SUM(B2:B{row - 1})";
            ws.Cell(row, 2).Style.Font.Bold = true;

            ws.Column(2).Style.NumberFormat.Format = "#,##0.00 ₽";

            ws.Columns().AdjustToContents();
            ws.SheetView.FreezeRows(1);
        }

        private static void StyleHeader(IXLRange range)
        {
            range.Style.Font.Bold = true;
            range.Style.Font.FontColor = XLColor.White;
            range.Style.Fill.BackgroundColor = XLColor.FromHtml("#3498DB");
            range.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
        }

        private static string GetProjectTypeText(ProjectType type) =>
            type switch
            {
                ProjectType.Split => "Сплит",
                ProjectType.PersonalGoal => "Личная цель",
                _ => type.ToString()
            };

        private static string GetCalculationTypeText(TargetCalculationType type) =>
            type switch
            {
                TargetCalculationType.FixedAmount => "Фиксированная сумма",
                TargetCalculationType.ExpenseList => "По списку расходов",
                _ => type.ToString()
            };

        private static string MakeSafeFileName(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return "Проект";

            foreach (char c in Path.GetInvalidFileNameChars())
            {
                value = value.Replace(c, '_');
            }

            return value.Trim();
        }
    }
}