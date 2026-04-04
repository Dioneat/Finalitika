using Finalitika10.Models;
using Finalitika10.Services.Interfaces.DocumentsService;
using System.Xml.Linq;

namespace Finalitika10.Services.DocumentsServices
{
    public class XmlGeneratorStrategy : IDocumentGeneratorStrategy
    {
        public ExportFormat Format => ExportFormat.Xml;

        public async Task<DocumentResult> GenerateAsync(DocumentRequest request)
        {
            if (request.DocumentType == "Ndfl3Declaration")
            {
                return GenerateNdfl3Xml(request);
            }

            var root = new XElement(request.DocumentType);
            foreach (var kvp in request.Data)
            {
                root.Add(new XElement(kvp.Key, kvp.Value));
            }
            var doc = new XDocument(new XDeclaration("1.0", "utf-8", "yes"), root);
            using var memoryStream = new MemoryStream();
            doc.Save(memoryStream);
            return new DocumentResult(Format, memoryStream.ToArray(), $"{request.DocumentType}.xml");
        }

        private DocumentResult GenerateNdfl3Xml(DocumentRequest request)
        {
            var d = request.Data;
            string dateStr = DateTime.Now.ToString("dd.MM.yyyy");
            string dateFileStr = DateTime.Now.ToString("yyyyMMdd");
            string guid = Guid.NewGuid().ToString().ToUpper();
            string taxCode = d.GetValueOrDefault("TaxCode", "0000");
            string inn = d.GetValueOrDefault("INN", "000000000000");
            string oktmo = d.GetValueOrDefault("OKTMO", "00000000");

            string fileId = $"NO_NDFL3_{taxCode}_{taxCode}_{inn}_{dateFileStr}_{guid}";

            decimal edu = decimal.TryParse(d.GetValueOrDefault("EduExp"), out var e) ? e : 0;
            decimal med = decimal.TryParse(d.GetValueOrDefault("MedExp"), out var m) ? m : 0;
            decimal fit = decimal.TryParse(d.GetValueOrDefault("FitExp"), out var f) ? f : 0;
            decimal totalSoc = edu + med + fit;

            var xmlDoc = new XDocument(new XDeclaration("1.0", "windows-1251", "yes"),
                new XElement("Файл", new XAttribute("ИдФайл", fileId),
                    new XElement("Документ",
                        new XAttribute("КНД", "1151020"),
                        new XAttribute("ДатаДок", dateStr),
                        new XAttribute("Период", "34"),
                        new XAttribute("ОтчетГод", d.GetValueOrDefault("Year", "2025")),
                        new XAttribute("КодНО", taxCode),
                        new XAttribute("НомКорр", "0"),

                        new XElement("СвНП",
                            new XElement("НПФЛ3",
                                new XAttribute("ДокПредст", "760"),
                                new XAttribute("Статус", "1"),
                                new XAttribute("Тлф", d.GetValueOrDefault("Phone", "")),
                                new XElement("ФИОФЛ",
                                    new XAttribute("Фамилия", d.GetValueOrDefault("LastName", "")),
                                    new XAttribute("Имя", d.GetValueOrDefault("FirstName", "")),
                                    new XAttribute("Отчество", d.GetValueOrDefault("Patronymic", ""))
                                ),
                                new XElement("ИННФЛ", inn)
                            )
                        ),
                        new XElement("Подписант", new XAttribute("ПрПодп", "1")),

                        new XElement("НДФЛ3",
                            new XElement("СумНалПу",
                                new XElement("СумНалПуИскл227",
                                    new XAttribute("КБК", "18210102010011000110"),
                                    new XAttribute("ОКТМО", oktmo),
                                    new XAttribute("ПодлУпл", "0"),
                                    new XAttribute("ПодлВозв", d.GetValueOrDefault("RefundAmount", "0"))
                                )
                            ),
                            new XElement("ЗаявРаспДС",
                                new XAttribute("Сумма", d.GetValueOrDefault("RefundAmount", "0") + ".00"),
                                new XElement("СвСчБанк",
                                    new XAttribute("БИК", d.GetValueOrDefault("BankBik", "")),
                                    new XAttribute("НомСч", d.GetValueOrDefault("BankAccount", ""))
                                )
                            ),
                            new XElement("НалБаза",
                                new XAttribute("ГрупДоход", "01"),
                                new XElement("РасчНалБаза",
                                    new XAttribute("СумДох", d.GetValueOrDefault("Income", "0") + ".00"),
                                    new XAttribute("СумДохНеНал", "0.00"),
                                    new XAttribute("СумДохНал", d.GetValueOrDefault("Income", "0") + ".00"),
                                    new XAttribute("СумНалВыч", "0.00"),
                                    new XAttribute("СумРасх", "0.00"), 
                                    new XAttribute("НалБаза", "0.00")
                                ),
                                new XElement("РасчНалПУ",
                                    new XAttribute("Исчисл", "13"),
                                    new XAttribute("Удерж", d.GetValueOrDefault("TaxWithheld", "0")),
                                    new XAttribute("СумУдержМат", "0"),
                                    new XAttribute("ТСУплПерЗач", "0"),
                                    new XAttribute("СумФиксАван", "0"),
                                    new XAttribute("УплИнПодлЗач", "0"),
                                    new XAttribute("УплПатентЗач", "0"),
                                    new XAttribute("ПодлУпл", "0"),
                                    new XAttribute("ПодлВозв", d.GetValueOrDefault("RefundAmount", "0")),
                                    new XAttribute("СумВозвУпр", "0")
                                )
                            ),
                            new XElement("ДоходИстРФ",
                                new XAttribute("ВидДоход", "026"),
                                new XAttribute("ОКТМО", oktmo),
                                new XAttribute("Доход", d.GetValueOrDefault("Income", "0") + ".00"),
                                new XAttribute("НалУдерж", d.GetValueOrDefault("TaxWithheld", "0")),
                                new XElement("ИстЮЛ",
                                    new XAttribute("Наим", d.GetValueOrDefault("EmployerName", "")),
                                    new XAttribute("ИННЮЛ", d.GetValueOrDefault("EmployerInn", "")),
                                    new XAttribute("КПП", d.GetValueOrDefault("EmployerKpp", ""))
                                )
                            ),
                            new XElement("ВычСтандСоц",
                                new XAttribute("ВычСтандСоц", totalSoc.ToString("0.00")),
                                new XElement("РасчВычСоц219.2",
                                    new XAttribute("СумОбуч", edu.ToString("0.00")),
                                    new XAttribute("СумМедУсл", med.ToString("0.00")),
                                    new XAttribute("СумФиз", fit.ToString("0.00")),
                                    new XAttribute("ОбщСумРасх", totalSoc.ToString("0.00")),
                                    new XAttribute("ОбщВычСоциал", totalSoc.ToString("0.00"))
                                )
                            )
                        )
                    )
                )
            );

            using var memoryStream = new MemoryStream();
            xmlDoc.Save(memoryStream);
            return new DocumentResult(Format, memoryStream.ToArray(), $"{fileId}.xml");
        }
    }
}
