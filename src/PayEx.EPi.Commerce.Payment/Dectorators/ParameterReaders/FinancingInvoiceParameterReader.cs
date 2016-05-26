using System;
using EPiServer.Logging.Compatibility;
using Mediachase.Commerce.Orders.Dto;
using PayEx.EPi.Commerce.Payment.Contracts;

namespace PayEx.EPi.Commerce.Payment.Dectorators.ParameterReaders
{
    internal class FinancingInvoiceParameterReader : IParameterReader
    {
        private readonly IParameterReader _parameterReader;
        private const string UseOnePhaseTransactionParameter = "UseOnePhaseTransaction";
        private const string GetLegalAddressParameter = "GetLegalAddress";
        private const string PaymentMethodCodeParameter = "PaymentMethodCode";

        private readonly ILog _log = LogManager.GetLogger(Constants.Logging.DefaultLoggerName);

        public FinancingInvoiceParameterReader(IParameterReader parameterReader)
        {
            _parameterReader = parameterReader;
        }

        public string GetPriceArgsList(PaymentMethodDto paymentMethodDto)
        {
            return _parameterReader.GetPriceArgsList(paymentMethodDto);
        }

        public string GetAdditionalValues(PaymentMethodDto paymentMethodDto)
        {
            var additionalValues = _parameterReader.GetAdditionalValues(paymentMethodDto);
            Models.PaymentMethods.MasterPass.ValidateMasterPassAdditionalValues(additionalValues);
            return additionalValues;
        }

        public bool UseOnePhaseTransaction(PaymentMethodDto paymentMethodDto)
        {
            try
            {
                var useOnePhaseTransactionValue = ParameterReader.GetParameterByName(paymentMethodDto, UseOnePhaseTransactionParameter).Value;
                bool useOnePhaseTransaction;
                if (bool.TryParse(useOnePhaseTransactionValue, out useOnePhaseTransaction))
                    return useOnePhaseTransaction;

                _log.Warn("Could not convert parameter value for UseOnePhaseTransaction to a boolean and is therefor handled as false.");
            }
            catch (ArgumentNullException)
            {
                return false;
            }

            return false;
        }

        public bool GetLegalAddress(PaymentMethodDto paymentMethodDto)
        {
            try
            {
                var getLegalAddressValue = ParameterReader.GetParameterByName(paymentMethodDto, GetLegalAddressParameter).Value;
                bool getLegalAddress;
                if (bool.TryParse(getLegalAddressValue, out getLegalAddress))
                    return getLegalAddress;

                _log.Warn("Could not convert parameter value for GetLegalAddress to a boolean and is therefor handled as false.");
            }
            catch (ArgumentNullException)
            {
                return false;
            }

            return false;
        }

        public string GetPaymentMethodCode(PaymentMethodDto paymentMethodDto)
        {
            return ParameterReader.GetParameterByName(paymentMethodDto, PaymentMethodCodeParameter).Value;
        }
    }
}
