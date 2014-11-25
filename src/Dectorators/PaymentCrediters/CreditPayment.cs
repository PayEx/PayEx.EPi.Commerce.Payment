﻿using Epinova.PayExProvider.Contracts;
using Epinova.PayExProvider.Models.PaymentMethods;
using Epinova.PayExProvider.Price;

namespace Epinova.PayExProvider.Dectorators.PaymentCrediters
{
    public class CreditPayment : IPaymentCreditor
    {
        private readonly IPaymentCreditor _paymentCreditor;
        private readonly ILogger _logger;
        private readonly IParameterReader _parameterReader;
        private readonly IPaymentManager _paymentManager;

        public CreditPayment(IPaymentCreditor paymentCreditor, ILogger logger, IPaymentManager paymentManager, IParameterReader parameterReader)
        {
            _paymentCreditor = paymentCreditor;
            _logger = logger;
            _parameterReader = parameterReader;
            _paymentManager = paymentManager;
        }

        public bool Credit(PaymentMethod currentPayment)
        {
            Mediachase.Commerce.Orders.Payment payment = (Mediachase.Commerce.Orders.Payment)currentPayment.Payment;
            
              int transactionId;
            if (!int.TryParse(payment.AuthorizationCode, out transactionId))
            {
                _logger.LogError(string.Format("Could not get PayEx Transaction Id from purchase order with ID: {0}", currentPayment.PurchaseOrder.Id));
                return false;
            }

            long amount = payment.Amount.RoundToLong();
            int vat = _parameterReader.GetVat(currentPayment.PaymentMethodDto);
            string transactionNumber = _paymentManager.Credit(transactionId, amount,
                currentPayment.PurchaseOrder.TrackingNumber, vat, string.Empty);

            bool success = false;
            if (!string.IsNullOrWhiteSpace(transactionNumber))
            {
                payment.TransactionID = transactionNumber;
                payment.AcceptChanges();
                success = true;
            }

            if (_paymentCreditor != null)
                return _paymentCreditor.Credit(currentPayment) && success;
            return success;
        }
    }
}
