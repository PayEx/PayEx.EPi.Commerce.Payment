
namespace EPiServer.Business.Commerce.Payment.PayEx.Contracts.Commerce
{
    public interface IAdditionalValuesFormatter
    {
        string Format(PayExPayment payExPayment);
    }
}
