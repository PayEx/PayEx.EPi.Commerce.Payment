
namespace Epinova.PayExProvider.Payment
{
    public enum TransactionStatus
    {
        Initialize = 1,
        Credit = 2,
        Authorize = 3,
        Capture = 6,
        Failure = 5,
        Other
    }
}
