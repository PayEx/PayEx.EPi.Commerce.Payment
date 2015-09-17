using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EPiServer.ServiceLocation;
using Mediachase.Commerce;
using Mediachase.Commerce.Catalog;
using Mediachase.Commerce.Catalog.Dto;
using Mediachase.Commerce.Catalog.Managers;
using Mediachase.Commerce.Markets;
using Mediachase.Commerce.Orders;
using PayEx.EPi.Commerce.Payment.Contracts.Commerce;
using PayEx.EPi.Commerce.Payment.Models;

namespace PayEx.EPi.Commerce.Payment.Commerce
{
    public class FinancingInvoiceOrderLineFormatter : IFinancialInvoicingOrderLineFormatter
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
            OrderForm orderForm = payExPayment.Parent;
            decimal lineitemTotal = 0;
            decimal lineitemVatTotal = 0;

            foreach (Shipment shipment in orderForm.Shipments)
            {
                foreach (LineItem item in Shipment.GetShipmentLineItems(shipment))
                {
                    // Try getting an address
                    OrderAddress address = GetAddressByName(orderForm, shipment.ShippingAddressId);
                    if (address != null) // no taxes if there is no address
                    {
                        // Try getting an entry
                        CatalogEntryDto entryDto = CatalogContext.Current.GetCatalogEntryDto(item.CatalogEntryId,
                            new CatalogEntryResponseGroup(CatalogEntryResponseGroup.ResponseGroup.Variations));

                        if (entryDto.CatalogEntry.Count > 0) // no entry, no tax category, no tax
                        {
                            CatalogEntryDto.VariationRow[] variationRows = entryDto.CatalogEntry[0].GetVariationRows();
                            if (variationRows.Length > 0)
                            {
                                string taxCategory =
                                    CatalogTaxManager.GetTaxCategoryNameById(variationRows[0].TaxCategoryId);
                                IMarket market =
                                    ServiceLocator.Current.GetInstance<IMarketService>()
                                        .GetMarket(orderForm.Parent.MarketId);
                                TaxValue[] taxes = OrderContext.Current.GetTaxes(Guid.Empty, taxCategory,
                                    market.DefaultLanguage.Name, address.CountryCode, address.State, address.PostalCode,
                                    address.RegionCode, String.Empty, address.City);

                                if (taxes.Length > 0)
                                {
                                    var quantity = Shipment.GetLineItemQuantity(shipment, item.LineItemId);

                                    // price exclude tax for 1 line item
                                    var unitprice = item.PlacedPrice -
                                                    (item.OrderLevelDiscountAmount +
                                                     item.LineItemDiscountAmount)/item.Quantity;

                                    var priceExcTax = unitprice*quantity;
                                    int vatRate = taxes.Where(tax => tax.TaxType == TaxType.SalesTax)
                                        .Aggregate(0,
                                            (current, tax) => current + Convert.ToInt16(Math.Floor(tax.Percentage)));
                                    decimal vatAmount = priceExcTax*((decimal) vatRate/100);
                                    lineitemTotal += unitprice;
                                    lineitemVatTotal += vatAmount;

                                    orderLines.Add(new OnlineInvoiceOrderLine()
                                    {
                                        UnitPrice = unitprice,
                                        ProductName = item.DisplayName,
                                        Quantity = quantity,
                                        VatRate = vatRate,
                                        VatAmount = vatAmount,
                                        Amount = priceExcTax + vatAmount
                                    });
                                }
                            }
                        }
                    }
                }

            }

            var restAmount = orderForm.Parent.Total - lineitemTotal - lineitemVatTotal;
            var restVatAmount = restAmount*((decimal) RestVatRate%100);
            orderLines.Add(new OnlineInvoiceOrderLine()
            {
                UnitPrice = lineitemTotal,
                ProductName = RestProductName,
                Quantity = 1,
                VatRate = RestVatRate,
                VatAmount = restVatAmount,
                Amount = restAmount
            });

            return orderLines;
        }

        private OrderAddress GetAddressByName(OrderForm form, string name)
        {
            return form.Parent.OrderAddresses.Cast<OrderAddress>().FirstOrDefault(address => address.Name.Equals(name));
        }
    }
}
