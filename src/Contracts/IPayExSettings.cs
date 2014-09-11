
namespace Epinova.PayExProvider.Contracts
{
    public interface IPayExSettings
    {
        long AccountNumber { get; }
        string PurchaseOperation { get; }
        string EncryptionKey { get; }
        string AuthorizationNoteTitle { get; }
        string AuthorizationNoteMessage { get;}
        string CaptureNoteTitle { get; }
        string CaptureNoteMessage { get; }
        string PaymentCancelUrl { get; }
        string PaymentReturnUrl { get; }
        string UserAgentFormat { get; }
        string DescriptionFormat { get; }
    }
}
