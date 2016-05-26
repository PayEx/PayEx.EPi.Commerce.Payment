using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Web;
using Mediachase.Commerce.Orders;
using Mediachase.Commerce.Orders.Dto;
using Mediachase.Commerce.Orders.Managers;
using PayEx.EPi.Commerce.Payment.Formatters;
using PayEx.EPi.Commerce.Payment.Models;
using PayEx.EPi.Commerce.Payment.Models.Result;

namespace PayEx.EPi.Commerce.Payment
{
    internal static class CartHelper
    {
        private const string CurrentCartKey = "CurrentCart";
        private const string CurrentContextKey = "CurrentContext";

        /// <summary>
        /// Uses parameterized thread to update the cart instance id otherwise will get an "workflow already existed" exception.
        /// Passes the cart and the current HttpContext as parameter in call back function to be able to update the instance id and also can update the HttpContext.Current if needed.
        /// </summary>
        /// <param name="cart">The cart to update.</param>
        /// <remarks>
        /// This method is used internal for payment methods which has redirect standard for processing payment e.g: PayPal, DIBS
        /// </remarks>
        internal static void UpdateCartInstanceId(Cart cart)
        {
            ParameterizedThreadStart threadStart = UpdateCartCallbackFunction;
            var thread = new Thread(threadStart);
            var cartInfo = new Hashtable();
            cartInfo[CurrentCartKey] = cart;
            cartInfo[CurrentContextKey] = HttpContext.Current;
            thread.Start(cartInfo);
            thread.Join();
        }

        /// <summary>
        /// Callback function for updating the cart. Before accept all changes of the cart, update the HttpContext.Current if it is null somehow.
        /// </summary>
        /// <param name="cartArgs">The cart agruments for updating.</param>
        private static void UpdateCartCallbackFunction(object cartArgs)
        {
            var cartInfo = cartArgs as Hashtable;
            if (cartInfo == null || !cartInfo.ContainsKey(CurrentCartKey))
            {
                return;
            }

            var cart = cartInfo[CurrentCartKey] as Cart;
            if (cart != null)
            {
                cart.InstanceId = Guid.NewGuid();
                if (HttpContext.Current == null && cartInfo.ContainsKey(CurrentContextKey))
                {
                    HttpContext.Current = cartInfo[CurrentContextKey] as HttpContext;
                }
                try
                {
                    cart.AcceptChanges();
                }
                catch
                {
                    //TODO
                }
            }
        }

        public static PayExAddress OrderAddress(Cart cart, PaymentInformation payment, InitializeResult result)
        {
            PayExAddress payexAddress = new PayExAddress(result.OrderRef.ToString());

            if (cart == null || cart.OrderForms == null || cart.OrderForms.Count == 0)
                return payexAddress;

            OrderForm orderForm = cart.OrderForms[0];

            OrderAddress billingAddress = cart.OrderAddresses.ToArray().FirstOrDefault(x => x.Name == orderForm.BillingAddressId);
            if (billingAddress != null)
                payexAddress.BillingAddress.Populate(billingAddress);

            if (orderForm.Shipments != null && orderForm.Shipments.Count > 0 && orderForm.Shipments[0] != null)
            {
                OrderAddress shippingAddress = cart.OrderAddresses.ToArray().FirstOrDefault(x => x.Name == orderForm.Shipments[0].ShippingAddressId);
                if (shippingAddress != null)
                    payexAddress.ShippingAddress.Populate(shippingAddress);
            }

            return payexAddress;
        }

        public static List<OrderLine> OrderLines(Cart cart, PaymentInformation payment, InitializeResult result)
        {
            List<OrderLine> orderLines = new List<OrderLine>();
            if (cart == null || cart.OrderForms == null || cart.OrderForms.Count == 0)
                return orderLines;

            OrderForm orderForm = cart.OrderForms[0];
            if (orderForm == null || orderForm.LineItems == null || orderForm.LineItems.Count == 0)
                return orderLines;

            foreach (LineItem lineItem in orderForm.LineItems)
            {
                orderLines.Add(new OrderLine(result.OrderRef.ToString(), lineItem.Code, lineItem.DisplayName, (int)lineItem.Quantity,
                    lineItem.ExtendedPrice.RoundToInt(), GetVatAmount(lineItem), GetVatPercentage(lineItem)));
            }

            decimal shippingVatPercentage = GetShippingVatPercentage();
            foreach (Shipment shipment in orderForm.Shipments)
            {
                decimal shippingVatAmount = cart.ShippingTotal * (shippingVatPercentage / 100);
                orderLines.Add(new OrderLine(result.OrderRef.ToString(), string.Empty, GetShippingMethodName(shipment), 1,
                    cart.ShippingTotal.RoundToInt(), shippingVatAmount, shippingVatPercentage));
            }
            return orderLines;
        }

        public static List<LineItem> GetLineItems(PayExPayment payExPayment)
        {
            var lineItems = new List<LineItem>();
            foreach (Shipment shipment in payExPayment.Parent.Shipments)
                lineItems.AddRange(Shipment.GetShipmentLineItems(shipment));

            return lineItems;
        }

        internal static decimal GetVatAmount(LineItem lineItem)
        {
            var vatObject = lineItem["LineItemVatAmount"];
            if (vatObject != null)
                return (decimal)vatObject;
            return 0;
        }

        internal static decimal GetVatPercentage(LineItem lineItem)
        {
            var vatPercentObject = lineItem["LineItemVatPercentage"];
            if (vatPercentObject != null)
                return (decimal)vatPercentObject;
            return 0;
        }

        internal static decimal GetShippingVatPercentage()
        {
            TaxDto taxDto = TaxManager.GetTaxDto(TaxType.ShippingTax);
            if (taxDto.TaxValue.Count == 0)
                return 0;
            return (decimal)taxDto.TaxValue[0].Percentage;
        }

        private static string GetShippingMethodName(Shipment shipment)
        {
            ShippingMethodDto.ShippingMethodRow shippingMethodRow = ShippingManager.GetShippingMethod(shipment.ShippingMethodId)
                .ShippingMethod.Single(s => s.ShippingMethodId == shipment.ShippingMethodId);
            if (shippingMethodRow != null)
                return shippingMethodRow.DisplayName;
            return string.Empty;
        }
    }
}
