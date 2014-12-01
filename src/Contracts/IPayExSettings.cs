
namespace Epinova.PayExProvider.Contracts
{
    public interface IPayExSettings
    {
        long AccountNumber { get; }
        string EncryptionKey { get; }
        string PaymentCancelUrl { get; }
        string PaymentReturnUrl { get; }
        string UserAgentFormat { get; }
        string DescriptionFormat { get; }
        string PayExCallbackIpAddress { get; }
        string SystemKeywordPrefix { get; }
        string InvoiceKeyword { get; }
    }
}
