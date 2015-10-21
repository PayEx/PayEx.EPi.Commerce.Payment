using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using log4net;
using Mediachase.Commerce.Orders;
using Mediachase.Data.Provider;
using PayEx.EPi.Commerce.Payment.Contracts;
using PayEx.EPi.Commerce.Payment.Models;
using PayEx.EPi.Commerce.Payment.Models.Result;
using PaymentMethod = PayEx.EPi.Commerce.Payment.Models.PaymentMethods.PaymentMethod;

namespace PayEx.EPi.Commerce.Payment.Dectorators.PaymentCompleters
{
    internal class UpdateTransactionDetails : IPaymentCompleter
    {
        private readonly IPaymentCompleter _paymentCompleter;
        private readonly IPaymentManager _paymentManager;
        protected readonly ILog Log = LogManager.GetLogger(Constants.Logging.DefaultLoggerName);

        public UpdateTransactionDetails(IPaymentCompleter paymentCompleter, IPaymentManager paymentManager)
        {
            _paymentCompleter = paymentCompleter;
            _paymentManager = paymentManager;
        }

        public PaymentCompleteResult Complete(PaymentMethod currentPayment, string orderRef)
        {
            Log.Info($"Updating transaction details for payment with ID:{currentPayment.Payment.Id} belonging to order with ID: {currentPayment.OrderGroupId}");
            var transactionString = ((Mediachase.Commerce.Orders.Payment)currentPayment.Payment).AuthorizationCode;
            Log.Info($"Transaction number is:{transactionString} for payment with ID:{currentPayment.Payment.Id} belonging to order with ID: {currentPayment.OrderGroupId}");

            int transactionNumber;
            if (!int.TryParse(transactionString, out transactionNumber))
            {
                Log.Error($"Could not parse Transaction number:{transactionString} to an Int for payment with ID:{currentPayment.Payment.Id} belonging to order with ID: {currentPayment.OrderGroupId}");
                return new PaymentCompleteResult();
            }

            var transactionDetails = _paymentManager.GetTransactionDetails(transactionNumber);
            var updated = false;
            if (transactionDetails != null)
                updated = UpdateOrderAddress(currentPayment, transactionDetails);

            var result = new PaymentCompleteResult { Success = true };
            if (_paymentCompleter != null)
                result = _paymentCompleter.Complete(currentPayment, orderRef);

            if (updated)
                Log.Info($"Successfully updated transaction details for payment with ID:{currentPayment.Payment.Id} belonging to order with ID: {currentPayment.OrderGroupId}");

            result.Success = updated;
            return result;
        }

        private bool UpdateOrderAddress(PaymentMethod currentPayment, TransactionResult transactionDetails2)
        {
            Log.Info($"Updating order address for payment with ID:{currentPayment.Payment.Id} belonging to order with ID: {currentPayment.OrderGroupId}");
            if (!currentPayment.RequireAddressUpdate)
            {
                Log.Info($"This payment method ({currentPayment.PaymentMethodCode}) does not require an order address update. Payment with ID:{currentPayment.Payment.Id} belonging to order with ID: {currentPayment.OrderGroupId}");
                return true;
            }

            var newAddress = currentPayment.GetAddressFromPayEx(transactionDetails2);
            if (newAddress == null)
                return false;

            var cart = currentPayment.Cart;
            var shippingAddress = GetShippingAddress(cart);
            if (shippingAddress == null)
            {
                Log.Error($"Could not update address for payment with ID:{currentPayment.Payment.Id} belonging to order with ID: {currentPayment.OrderGroupId}. Reason: Shipping address was not found!");
                return false;
            }

            var propertiesToUpdate = new Dictionary<string, string>()
            {
                {GetPropertyName(() => shippingAddress.FirstName), newAddress.FirstName},
                {GetPropertyName(() => shippingAddress.LastName), newAddress.LastName},
                {GetPropertyName(() => shippingAddress.Line1), newAddress.Line1},
                {GetPropertyName(() => shippingAddress.PostalCode), newAddress.PostCode},
                {GetPropertyName(() => shippingAddress.City), newAddress.City},
                {GetPropertyName(() => shippingAddress.CountryName), newAddress.Country},
                {GetPropertyName(() => shippingAddress.Email), newAddress.Email},
            };

            var updated = UpdatePropertyValues(shippingAddress, propertiesToUpdate);

            if (!string.IsNullOrWhiteSpace(newAddress.Fullname))
            {
                Log.Info($"Setting customer name of cart to {newAddress.Fullname} on payment with ID:{currentPayment.Payment.Id} belonging to order with ID: {currentPayment.OrderGroupId}");
                cart.CustomerName = newAddress.Fullname;
            }

            try
            {
                if (!updated) return true;

                using (var scope = new TransactionScope())
                {
                    shippingAddress.AcceptChanges();
                    cart.AcceptChanges();
                    scope.Complete();
                }
                return true;
            }
            catch (Exception e)
            {
                Log.Error("Could not update address for payment. See next log statement for more information", e);
                Log.Error($"Could not update address for payment with ID:{currentPayment.Payment.Id} belonging to order with ID: {currentPayment.OrderGroupId}");
                return false;
            }
        }

        private bool UpdatePropertyValues(OrderAddress address, Dictionary<string, string> properties)
        {
            var updated = false;
            foreach (var property in properties)
            {
                if (string.IsNullOrWhiteSpace(property.Value))
                    continue;

                var propertyInfo = address.GetType().GetProperty(property.Key);
                propertyInfo.SetValue(address, property.Value, null);
                updated = true;
            }
            return updated;
        }

        public string GetPropertyName<T>(Expression<Func<T>> propertyExpression)
        {
            return (propertyExpression.Body as MemberExpression).Member.Name;
        }

        public OrderAddress GetShippingAddress(OrderGroup orderGroup)
        {
            var orderForm = orderGroup.OrderForms.ToArray().First();
            if (orderForm?.Shipments == null || orderForm.Shipments.Count == 0 || string.IsNullOrEmpty(orderForm.Shipments[0].ShippingAddressId))
                return null;
            return orderGroup.OrderAddresses.ToArray().FirstOrDefault(x => x.Name.Equals(orderForm.Shipments[0].ShippingAddressId, StringComparison.OrdinalIgnoreCase));
        }
    }
}
