using System;
using System.Linq;
using EPiServer.Business.Commerce.Payment.PayEx.Contracts;
using EPiServer.Business.Commerce.Payment.PayEx.Models;
using EPiServer.Business.Commerce.Payment.PayEx.Models.Result;
using Mediachase.Commerce.Orders;
using Mediachase.Data.Provider;
using PaymentMethod = EPiServer.Business.Commerce.Payment.PayEx.Models.PaymentMethods.PaymentMethod;

namespace EPiServer.Business.Commerce.Payment.PayEx.Dectorators.PaymentCompleters
{
    internal class UpdateTransactionDetails : IPaymentCompleter
    {
        private readonly IPaymentCompleter _paymentCompleter;
        private readonly IPaymentManager _paymentManager;
        private readonly ILogger _logger;

        public UpdateTransactionDetails(IPaymentCompleter paymentCompleter, IPaymentManager paymentManager, ILogger logger)
        {
            _paymentCompleter = paymentCompleter;
            _paymentManager = paymentManager;
            _logger = logger;
        }

        public PaymentCompleteResult Complete(PaymentMethod currentPayment, string orderRef)
        {
            string transactionString = ((Mediachase.Commerce.Orders.Payment)currentPayment.Payment).AuthorizationCode;
            int transactionNumber;
            if (!Int32.TryParse(transactionString, out transactionNumber))
                return new PaymentCompleteResult();

            TransactionResult transactionDetails = _paymentManager.GetTransactionDetails(transactionNumber);
            bool updated = UpdateOrderAddress(currentPayment, transactionDetails);

            PaymentCompleteResult result = new PaymentCompleteResult { Success = true };
            if (_paymentCompleter != null)
                result = _paymentCompleter.Complete(currentPayment, orderRef);

            result.Success = updated;
            return result;
        }

        private bool UpdateOrderAddress(PaymentMethod currentPayment, TransactionResult transactionDetails)
        {
            bool updated = false;
            PurchaseOrder purchaseOrder = currentPayment.PurchaseOrder;
            OrderAddress billingAddress = GetBillingAddress(purchaseOrder);

            if (!string.IsNullOrWhiteSpace(transactionDetails.CustomerName))
            {
                purchaseOrder.CustomerName = transactionDetails.CustomerName;
                string[] names = transactionDetails.CustomerName.Split(' ');
                billingAddress.FirstName = names[0];
                if (names.Length > 1)
                    billingAddress.LastName = string.Join(" ", names.Skip(1));
                updated = true;
            }

            if (!string.IsNullOrWhiteSpace(transactionDetails.Address))
            {
                billingAddress.Line1 = transactionDetails.Address;
                updated = true;
            }

            if (!string.IsNullOrWhiteSpace(transactionDetails.PostNumber))
            {
                billingAddress.PostalCode = transactionDetails.PostNumber;
                updated = true;
            }

            if (!string.IsNullOrWhiteSpace(transactionDetails.City))
            {
                billingAddress.City = transactionDetails.City;
                updated = true;
            }

            if (!string.IsNullOrWhiteSpace(transactionDetails.Country))
            {
                billingAddress.CountryName = transactionDetails.Country;
                updated = true;
            }

            try
            {
                if (updated)
                {
                    using (TransactionScope scope = new TransactionScope())
                    {

                        billingAddress.AcceptChanges();
                        purchaseOrder.AcceptChanges();
                        scope.Complete();
                    }
                }
                return true;
            }
            catch (Exception e)
            {
                _logger.LogError("Could not update address on purchase order", e);
                return false;
            }
        }

        public OrderAddress GetBillingAddress(OrderGroup orderGroup)
        {
            OrderForm orderForm = orderGroup.OrderForms.First();
            if (orderForm == null || string.IsNullOrEmpty(orderForm.BillingAddressId))
                return null;
            return orderGroup.OrderAddresses.FirstOrDefault(x => x.Name.Equals(orderForm.BillingAddressId, StringComparison.OrdinalIgnoreCase));
        }
    }
}
