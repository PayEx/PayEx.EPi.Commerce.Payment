using Epinova.PayExProvider.Models;
using Epinova.PayExProvider.Payment;
using Epinova.PayExProvider.Price;
using EPiServer;
using EPiServer.Commerce.Catalog.ContentTypes;
using EPiServer.Commerce.Catalog.Linking;
using EPiServer.Core;
using EPiServer.ServiceLocation;
using Mediachase.Commerce.Catalog;
using Mediachase.Commerce.Orders;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Web;
using Mediachase.Commerce.Orders.Dto;
using Mediachase.Commerce.Orders.Managers;

namespace Epinova.PayExProvider
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
                catch (System.Exception ex)
                {
                    //TODO
                }
            }
        }

        public static PayExAddress OrderAddress(Cart cart, PaymentInformation payment, InitializeResult result)
        {
            PayExAddress payexAddress = new PayExAddress(payment.AccountNumber, result.OrderRef.ToString(), payment.EncryptionKey);

            if (cart == null || cart.OrderForms == null || !cart.OrderForms.Any())
                return payexAddress;

            OrderForm orderForm = cart.OrderForms[0];

            OrderAddress billingAddress = cart.OrderAddresses.FirstOrDefault(x => x.Name == orderForm.BillingAddressId);
            if (billingAddress != null)
                payexAddress.BillingAddress.Populate(billingAddress);

            if (orderForm.Shipments != null && orderForm.Shipments.Any() && orderForm.Shipments[0] != null)
            {
                OrderAddress shippingAddress = cart.OrderAddresses.FirstOrDefault(x => x.Name == orderForm.Shipments[0].ShippingAddressId);
                if (shippingAddress != null)
                    payexAddress.ShippingAddress.Populate(shippingAddress);
            }

            return payexAddress;
        }

        public static List<OrderLine> OrderLines(Cart cart, PaymentInformation payment, InitializeResult result)
        {
            List<OrderLine> orderLines = new List<OrderLine>();
            PriceFormatter priceFormatter = new PriceFormatter();

            if (cart == null || cart.OrderForms == null || !cart.OrderForms.Any())
                return orderLines;

            OrderForm orderForm = cart.OrderForms[0];
            if (orderForm == null || orderForm.LineItems == null || !orderForm.LineItems.Any())
                return orderLines;

            foreach (LineItem lineItem in orderForm.LineItems)
            {
                orderLines.Add(new OrderLine(payment.AccountNumber, result.OrderRef.ToString(), lineItem.CatalogEntryId, GetProductName(lineItem.CatalogEntryId), (int)lineItem.Quantity,
                    priceFormatter.RoundToInt(lineItem.ExtendedPrice), GetVatAmount(lineItem), GetVatPercentage(lineItem), payment.EncryptionKey));
            }

            foreach (Shipment shipment in orderForm.Shipments)
            {
                orderLines.Add(new OrderLine(payment.AccountNumber, result.OrderRef.ToString(), string.Empty, GetShippingMethodName(shipment), 1,
                    priceFormatter.RoundToInt(cart.ShippingTotal), 0, 0, payment.EncryptionKey));
            }
            return orderLines;
        }

        private static string GetProductName(string variantCode)
        {
            var referenceConverter = ServiceLocator.Current.GetInstance<ReferenceConverter>();
            var linksRepository = ServiceLocator.Current.GetInstance<ILinksRepository>();
            ContentReference variantReference = referenceConverter.GetContentLink(variantCode);
            IEnumerable<Relation> relationsByTarget = linksRepository.GetRelationsByTarget(variantReference).Where(x => x is ProductVariation).ToList();

            if (!relationsByTarget.Any())
                return null;

            Relation relation = relationsByTarget.First();
            var contentLoader = ServiceLocator.Current.GetInstance<IContentLoader>();
            return contentLoader.Get<ProductContent>(relation.Source).DisplayName;
        }

        private static decimal GetVatAmount(LineItem lineItem)
        {
            var vatObject = lineItem["LineItemVatAmount"];
            if (vatObject != null)
                return (decimal)vatObject;
            return 0;
        }

        private static decimal GetVatPercentage(LineItem lineItem)
        {
            var vatPercentObject = lineItem["LineItemVatPercentage"];
            if (vatPercentObject != null)
                return (decimal)vatPercentObject;
            return 0;
        }

        private static string GetShippingMethodName(Shipment shipment)
        {
            ShippingMethodDto.ShippingMethodRow shippingMethodRow = ShippingManager.GetShippingMethod(shipment.ShippingMethodId)
                .ShippingMethod.Single(s => s.ShippingMethodId == shipment.ShippingMethodId);
            if (shippingMethodRow != null)
                return shippingMethodRow.DisplayName;
            return "Frakt";
        }
    }
}
