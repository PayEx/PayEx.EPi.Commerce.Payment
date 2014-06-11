
namespace Epinova.PayExProvider.Contracts
{
    public interface ISettings
    {
        long AccountNumber { get; }
        string PurchaseOperation { get; }
        string EncryptionKey { get; }
        string AuthorizationNoteTitle { get; }
        string AuthorizationNoteMessage { get;}
        string CaptureNoteTitle { get; }
        string CaptureNoteMessage { get; }
    }
}
