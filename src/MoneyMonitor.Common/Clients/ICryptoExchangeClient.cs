using System.Collections.Generic;
using System.Threading.Tasks;
using MoneyMonitor.Common.Models;

namespace MoneyMonitor.Common.Clients
{
    public interface ICryptoExchangeClient
    {
        Task<List<ExchangeBalance>> GetBalances();
    }
}