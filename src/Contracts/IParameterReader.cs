using Mediachase.Commerce.Orders.Dto;

namespace EPiServer.Business.Commerce.Payment.PayEx.Contracts
{
    internal interface IParameterReader
    {
        int GetVat(PaymentMethodDto paymentMethodDto);
        string GetDefaultView(PaymentMethodDto paymentMethodDto);
        string GetPriceArgsList(PaymentMethodDto paymentMethodDto);
        string GetAdditionalValues(PaymentMethodDto paymentMethodDto);
        string GetPurchaseOperation(PaymentMethodDto paymentMethodDto);
    }
}
