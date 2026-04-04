using Google.Protobuf.Collections;
using Tinkoff.InvestApi.V1;

namespace Finalitika10.Models
{
    public class AssetCategory
    {
        public string Name { get; set; } = string.Empty;
        public decimal Percent { get; set; }
        public decimal Amount { get; set; }
        public Color Color { get; set; } = Colors.Gray;

        public double ProgressValue => (double)(Percent / 100m);
        public string DisplayPercent => $"{Percent:F1}%";
        public string DisplayValue => $"{Amount:N2} ₽";
    }

    public class OperationData
    {
        public string Date { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public string Asset { get; set; } = string.Empty;
        public string Amount { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
    }

    public class PortfolioData
    {
        public decimal TotalShares { get; set; }
        public decimal TotalBonds { get; set; }
        public decimal TotalEtfs { get; set; }
        public decimal TotalCurrencies { get; set; }
        public decimal TotalAmountPortfolio { get; set; }

        public Quotation? ExpectedYield { get; set; }
        public Quotation? DailyYieldRelative { get; set; }

        public decimal DailyYield { get; set; }

        public List<PositionData> Positions { get; set; } = new();
    }

    public class PositionData
    {
        public string Figi { get; set; } = string.Empty;
        public string InstrumentType { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;

        public decimal Quantity { get; set; }
        public decimal AveragePositionPrice { get; set; }
        public decimal CurrentPrice { get; set; }

        public decimal MarketValue => Quantity * CurrentPrice;

        public RepeatedField<Dividend> Dividends { get; set; } = new();
    }
}