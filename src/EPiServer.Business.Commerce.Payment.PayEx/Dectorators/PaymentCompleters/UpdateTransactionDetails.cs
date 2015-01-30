using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using EPiServer.Business.Commerce.Payment.PayEx.Contracts;
using EPiServer.Business.Commerce.Payment.PayEx.Models;
using EPiServer.Business.Commerce.Payment.PayEx.Models.Result;
using log4net;
using Mediachase.Commerce.Orders;
using Mediachase.Data.Provider;
using PaymentMethod = EPiServer.Business.Commerce.Payment.PayEx.Models.PaymentMethods.PaymentMethod;

namespace EPiServer.Business.Commerce.Payment.PayEx.Dectorators.PaymentCompleters
{
    public class UpdateTransactionDetails : IPaymentCompleter
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

            Cart cart = currentPayment.Cart;
            OrderAddress shippingAddress = GetShippingAddress(cart);

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
                cart.CustomerName = newAddress.Fullname;

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
                Log.Error("Could not update address on purchase order", e);
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
