using Mediachase.Commerce.Orders.Dto;

namespace EPiServer.Business.Commerce.Payment.PayEx.Contracts
{
    internal interface IParameterReader
    {
        string GetPriceArgsList(PaymentMethodDto paymentMethodDto);
        string GetAdditionalValues(PaymentMethodDto paymentMethodDto);
    }
}
