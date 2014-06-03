
using Epinova.PayExProvider.Models;

namespace Epinova.PayExProvider.Contracts
{
    public interface IPaymentManager
    {
        string Initialize(Payment payment);
    }
}
