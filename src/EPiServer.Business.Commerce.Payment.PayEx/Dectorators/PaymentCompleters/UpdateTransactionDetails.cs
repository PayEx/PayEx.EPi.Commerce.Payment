using System;
using System.Linq;
using EPiServer.Business.Commerce.Payment.PayEx.Contracts;
using EPiServer.Business.Commerce.Payment.PayEx.Models;
using EPiServer.Business.Commerce.Payment.PayEx.Models.PaymentMethods;
using EPiServer.Business.Commerce.Payment.PayEx.Models.Result;
using Mediachase.Commerce.Orders;
using Mediachase.Data.Provider;
using PaymentMethod = EPiServer.Business.Commerce.Payment.PayEx.Models.PaymentMethods.PaymentMethod;

namespace EPiServer.Business.Commerce.Payment.PayEx.Dectorators.PaymentCompleters
{
    public class UpdateTransactionDetails : IPaymentCompleter
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
            bool updated = false;
            if (transactionDetails != null)
                updated = UpdateOrderAddress(currentPayment, transactionDetails);

            PaymentCompleteResult result = new PaymentCompleteResult { Success = true };
            if (_paymentCompleter != null)
                result = _paymentCompleter.Complete(currentPayment, orderRef);

            result.Success = updated;
            return result;
        }

        private bool UpdateOrderAddress(PaymentMethod currentPayment, TransactionResult transactionDetails2)
        {
            if (!currentPayment.RequireAddressUpdate)
                return true;

            Address newAddress = currentPayment.GetAddressFromPayEx(transactionDetails2);
            if (newAddress == null)
                return false;

            bool updated = false;
            Cart cart = currentPayment.Cart;
            OrderAddress shippingAddress = GetShippingAddress(cart);

            if (!string.IsNullOrWhiteSpace(newAddress.FirstName))
            {
                shippingAddress.FirstName = newAddress.FirstName;
                updated = true;
            }

            if (!string.IsNullOrWhiteSpace(newAddress.LastName))
            {
                shippingAddress.LastName = newAddress.LastName;
                updated = true;
            }

            if (!string.IsNullOrWhiteSpace(newAddress.Fullname))
                cart.CustomerName = newAddress.Fullname;

            if (!string.IsNullOrWhiteSpace(newAddress.Line1))
            {
                shippingAddress.Line1 = newAddress.Line1;
                updated = true;
            }

            if (!string.IsNullOrWhiteSpace(newAddress.PostCode))
            {
                shippingAddress.PostalCode = newAddress.PostCode;
                updated = true;
            }

            if (!string.IsNullOrWhiteSpace(newAddress.City))
            {
                shippingAddress.City = newAddress.City;
                updated = true;
            }

            if (!string.IsNullOrWhiteSpace(newAddress.Country))
            {
                shippingAddress.CountryName = newAddress.Country;
                updated = true;
            }

            if (!string.IsNullOrWhiteSpace(newAddress.Email))
            {
                shippingAddress.Email = newAddress.Email;
                updated = true;
            }

            try
            {
                if (updated)
                {
                    using (TransactionScope scope = new TransactionScope())
                    {
                        shippingAddress.AcceptChanges();
                        cart.AcceptChanges();
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

        public OrderAddress GetShippingAddress(OrderGroup orderGroup)
        {
            OrderForm orderForm = orderGroup.OrderForms.ToArray().First();
            if (orderForm == null || orderForm.Shipments == null || orderForm.Shipments.Count == 0 || string.IsNullOrEmpty(orderForm.Shipments[0].ShippingAddressId))
                return null;
            return orderGroup.OrderAddresses.ToArray().FirstOrDefault(x => x.Name.Equals(orderForm.Shipments[0].ShippingAddressId, StringComparison.OrdinalIgnoreCase));
        }
    }
}
