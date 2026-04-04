using Finalitika10.Models;

namespace Finalitika10.Services.Investments
{
    public interface IInvestmentService
    {
        Task<PortfolioData> GetPortfolioAsync();
    }
}
