using System;
using EPiServer.Business.Commerce.Payment.PayEx.Contracts;
using Mediachase.Commerce.Orders.Dto;

namespace EPiServer.Business.Commerce.Payment.PayEx
{
    internal class ParameterReader : IParameterReader
    {
        public const string PriceListArgsParameter = "PriceListArgs";
        public const string AdditionalValuesParameter = "AdditionalValues";

        public string GetAdditionalValues(PaymentMethodDto paymentMethodDto)
        {
            return GetParameterByName(paymentMethodDto, AdditionalValuesParameter).Value;
        }

        public string GetPriceArgsList(PaymentMethodDto paymentMethodDto)
        {
            return GetParameterByName(paymentMethodDto, PriceListArgsParameter).Value;
        }

        internal PaymentMethodDto.PaymentMethodParameterRow GetParameterByName(PaymentMethodDto paymentMethodDto, string name)
        {
            PaymentMethodDto.PaymentMethodParameterRow[] rowArray = (PaymentMethodDto.PaymentMethodParameterRow[])paymentMethodDto.PaymentMethodParameter.Select(string.Format("Parameter = '{0}'", name));
            if (rowArray.Length > 0)
                return rowArray[0];
            throw new ArgumentNullException("Parameter named " + name + " for PayEx payment cannot be null");
        }
    }
}
