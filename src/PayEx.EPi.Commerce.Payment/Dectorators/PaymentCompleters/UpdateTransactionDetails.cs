using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using EPiServer.Logging.Compatibility;
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
            Log.InfoFormat("Updating transaction details for payment with ID:{0} belonging to order with ID: {1}", currentPayment.Payment.Id, currentPayment.OrderGroupId);
            string transactionString = ((Mediachase.Commerce.Orders.Payment)currentPayment.Payment).AuthorizationCode;
            Log.InfoFormat("Transaction number is:{0} for payment with ID:{1} belonging to order with ID: {2}", transactionString, currentPayment.Payment.Id, currentPayment.OrderGroupId);

            int transactionNumber;
            if (!Int32.TryParse(transactionString, out transactionNumber))
            {
                Log.ErrorFormat("Could not parse Transaction number:{0} to an Int for payment with ID:{1} belonging to order with ID: {2}", transactionString, currentPayment.Payment.Id, currentPayment.OrderGroupId);
                return new PaymentCompleteResult();
            }

            TransactionResult transactionDetails = _paymentManager.GetTransactionDetails(transactionNumber);
            bool updated = false;
            if (transactionDetails != null)
                updated = UpdateOrderAddress(currentPayment, transactionDetails);

            PaymentCompleteResult result = new PaymentCompleteResult { Success = true };
            if (_paymentCompleter != null)
                result = _paymentCompleter.Complete(currentPayment, orderRef);

            if (updated)
                Log.InfoFormat("Successfully updated transaction details for payment with ID:{0} belonging to order with ID: {1}", currentPayment.Payment.Id, currentPayment.OrderGroupId);

            result.Success = updated;
            return result;
        }

        private bool UpdateOrderAddress(PaymentMethod currentPayment, TransactionResult transactionDetails2)
        {
            Log.InfoFormat("Updating order address for payment with ID:{0} belonging to order with ID: {1}", currentPayment.Payment.Id, currentPayment.OrderGroupId);
            if (!currentPayment.RequireAddressUpdate)
            {
                Log.InfoFormat("This payment method ({0}) does not require an order address update. Payment with ID:{1} belonging to order with ID: {2}",
                    currentPayment.PaymentMethodCode, currentPayment.Payment.Id, currentPayment.OrderGroupId);
                return true;
            }

            Address newAddress = currentPayment.GetAddressFromPayEx(transactionDetails2);
            if (newAddress == null)
                return false;

            Cart cart = currentPayment.Cart;
            OrderAddress shippingAddress = GetShippingAddress(cart);
            if (shippingAddress == null)
            {
                Log.ErrorFormat("Could not update address for payment with ID:{0} belonging to order with ID: {1}. Reason: Shipping address was not found!", currentPayment.Payment.Id, currentPayment.OrderGroupId);
                return false;
            }

            Dictionary<string, string> propertiesToUpdate = new Dictionary<string, string>()
            {
                {GetPropertyName(() => shippingAddress.FirstName), newAddress.FirstName},
                {GetPropertyName(() => shippingAddress.LastName), newAddress.LastName},
                {GetPropertyName(() => shippingAddress.Line1), newAddress.Line1},
                {GetPropertyName(() => shippingAddress.PostalCode), newAddress.PostCode},
                {GetPropertyName(() => shippingAddress.City), newAddress.City},
                {GetPropertyName(() => shippingAddress.CountryName), newAddress.Country},
                {GetPropertyName(() => shippingAddress.Email), newAddress.Email},
            };

            bool updated = UpdatePropertyValues(shippingAddress, propertiesToUpdate);

            if (!string.IsNullOrWhiteSpace(newAddress.Fullname))
            {
                Log.InfoFormat("Setting customer name of cart to {0} on payment with ID:{1} belonging to order with ID: {2}",
                    newAddress.Fullname, currentPayment.Payment.Id, currentPayment.OrderGroupId);
                cart.CustomerName = newAddress.Fullname;
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
                Log.Error("Could not update address for payment. See next log statement for more information", e);
                Log.ErrorFormat("Could not update address for payment with ID:{0} belonging to order with ID: {1}", currentPayment.Payment.Id, currentPayment.OrderGroupId);
                return false;
            }
        }

        private bool UpdatePropertyValues(OrderAddress address, Dictionary<string, string> properties)
        {
            bool updated = false;
            foreach (KeyValuePair<string, string> property in properties)
            {
                if (string.IsNullOrWhiteSpace(property.Value))
                    continue;

                PropertyInfo propertyInfo = address.GetType().GetProperty(property.Key);
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
            OrderForm orderForm = orderGroup.OrderForms.ToArray().First();
            if (orderForm == null || orderForm.Shipments == null || orderForm.Shipments.Count == 0 || string.IsNullOrEmpty(orderForm.Shipments[0].ShippingAddressId))
                return null;
            return orderGroup.OrderAddresses.ToArray().FirstOrDefault(x => x.Name.Equals(orderForm.Shipments[0].ShippingAddressId, StringComparison.OrdinalIgnoreCase));
        }
    }
}
