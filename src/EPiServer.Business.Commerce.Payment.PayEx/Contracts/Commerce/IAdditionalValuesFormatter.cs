
namespace EPiServer.Business.Commerce.Payment.PayEx.Contracts.Commerce
{
    public interface IAdditionalValuesFormatter
    {
        /// <summary>
        /// Returns the additionalValues parameter as a a formatted string
        /// </summary>
        string Format(PayExPayment payExPayment);
    }
}
