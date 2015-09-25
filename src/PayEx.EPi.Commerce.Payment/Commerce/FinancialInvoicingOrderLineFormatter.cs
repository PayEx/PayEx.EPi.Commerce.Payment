using System;
using System.Collections.Generic;
using System.Linq;
using Mediachase.Commerce.Orders;
using PayEx.EPi.Commerce.Payment.Contracts.Commerce;
using PayEx.EPi.Commerce.Payment.Formatters;
using PayEx.EPi.Commerce.Payment.Models;

namespace PayEx.EPi.Commerce.Payment.Commerce
{
    public class FinancialInvoicingOrderLineFormatter : IFinancialInvoicingOrderLineFormatter
    {
        public virtual string RestProductName
        {
            get { return "Frakt og rabatt"; }
        }

        public virtual int RestVatRate
        {
            get
            {
                return 0;
            }
        }

        public virtual bool IncludeOrderLines { get; set; }

        public virtual List<OnlineInvoiceOrderLine> CreateOrderLines(PayExPayment payExPayment)
        {
            List<OnlineInvoiceOrderLine> orderLines = new List<OnlineInvoiceOrderLine>();
            var lineItems = CartHelper.GetLineItems(payExPayment);
            decimal lineItemTotal = 0;
            foreach (var item in lineItems)
            {
                var unitprice = item.PlacedPrice -
                                (item.OrderLevelDiscountAmount +
                                 item.LineItemDiscountAmount) / item.Quantity;
                lineItemTotal += item.ExtendedPrice;

                orderLines.Add(new OnlineInvoiceOrderLine()
                {
                    UnitPrice = unitprice.RoundToTwoDecimal(),
                    ProductName = item.DisplayName,
                    Quantity = (int)(Math.Round(item.Quantity)),
                    VatRate = (int)Math.Round(CartHelper.GetVatPercentage(item)),
                    VatAmount = CartHelper.GetVatAmount(item).RoundToTwoDecimal(),
                    Amount = item.ExtendedPrice.RoundToTwoDecimal()
                });                
            }

            var restAmount = payExPayment.Parent.Parent.Total - lineItemTotal;
            var restVatAmount = restAmount*((decimal) RestVatRate%100);
            orderLines.Add(new OnlineInvoiceOrderLine()
            {
                UnitPrice = restAmount.RoundToTwoDecimal(),
                ProductName = RestProductName,
                Quantity = 1,
                VatRate = RestVatRate,
                VatAmount = restVatAmount.RoundToTwoDecimal(),
                Amount = restAmount.RoundToTwoDecimal()
            });

            return orderLines;
        }

        private OrderAddress GetAddressByName(OrderForm form, string name)
        {
            return form.Parent.OrderAddresses.Cast<OrderAddress>().FirstOrDefault(address => address.Name.Equals(name));
        }
    }
}
