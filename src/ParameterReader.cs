using System;
using Epinova.PayExProvider.Contracts;
using Mediachase.Commerce.Orders.Dto;

namespace Epinova.PayExProvider
{
    public class ParameterReader : IParameterReader
    {
        public const string VatParameter = "Vat";
        public const string PriceListArgsParameter = "PriceListArgs";
        public const string AdditionalValuesParameter = "AdditionalValues";
        public const string DefaultViewParameter = "DefaultView";

        public string GetAdditionalValues(PaymentMethodDto paymentMethodDto)
        {
            return GetParameterByName(paymentMethodDto, AdditionalValuesParameter).Value;
        }

        public string GetPriceArgsList(PaymentMethodDto paymentMethodDto)
        {
            return GetParameterByName(paymentMethodDto, PriceListArgsParameter).Value;
        }

        public int GetVat(PaymentMethodDto paymentMethodDto)
        {
            int vat;
            if (int.TryParse(GetParameterByName(paymentMethodDto, VatParameter).Value, out vat))
                return vat;
            return 0;
        }

        public string GetDefaultView(PaymentMethodDto paymentMethodDto)
        {
            return GetParameterByName(paymentMethodDto, DefaultViewParameter).Value;
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
