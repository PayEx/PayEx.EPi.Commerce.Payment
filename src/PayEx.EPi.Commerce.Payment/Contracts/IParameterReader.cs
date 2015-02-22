using Mediachase.Commerce.Orders.Dto;

namespace PayEx.EPi.Commerce.Payment.Contracts
{
    internal interface IParameterReader
    {
        string GetPriceArgsList(PaymentMethodDto paymentMethodDto);
        string GetAdditionalValues(PaymentMethodDto paymentMethodDto);
    }
}
