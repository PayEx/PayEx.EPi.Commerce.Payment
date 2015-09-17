using System;
using log4net;
using Mediachase.Commerce.Orders.Dto;
using PayEx.EPi.Commerce.Payment.Contracts;

namespace PayEx.EPi.Commerce.Payment.Dectorators.ParameterReaders
{
    internal class MasterPassParameterReader : IParameterReader
    {
        private readonly IParameterReader _parameterReader;
        private const string UseBestPracticeFlowParameter = "UseBestPracticeFlow";
        private readonly ILog _log = LogManager.GetLogger(Constants.Logging.DefaultLoggerName);

        public MasterPassParameterReader(IParameterReader parameterReader)
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

        public bool UseBestPracticeFlow(PaymentMethodDto paymentMethodDto)
        {
            try
            {
                var useBestPracticeFlowValue = ParameterReader.GetParameterByName(paymentMethodDto, UseBestPracticeFlowParameter).Value;
                bool useBestPracticeFlow;
                if (bool.TryParse(useBestPracticeFlowValue, out useBestPracticeFlow))
                    return useBestPracticeFlow;

                _log.Warn("Could not convert parameter value for UseBestPracticeFlow to a boolean and is therefor handled as false.");
            }
            catch (ArgumentNullException)
            {
                return false;
            }

            return false;
        }
    }
}
