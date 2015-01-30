
namespace EPiServer.Business.Commerce.Payment.PayEx.Contracts
{
    interface IPayExSettings
    {
        long AccountNumber { get; }
        string EncryptionKey { get; }
        bool IncludeOrderLines { get; }
        bool DisablePaymentMethodCreation { get; }
        bool IncludeCustomerAddress { get; }
    }
}
