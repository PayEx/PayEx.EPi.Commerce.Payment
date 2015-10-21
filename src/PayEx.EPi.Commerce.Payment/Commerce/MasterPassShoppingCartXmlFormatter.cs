using System.Linq;
using PayEx.EPi.Commerce.Payment.Contracts.Commerce;
using PayEx.EPi.Commerce.Payment.Formatters;
using PayEx.EPi.Commerce.Payment.Models;

namespace PayEx.EPi.Commerce.Payment.Commerce
{
    public class MasterPassShoppingCartXmlFormatter : IMasterPassShoppingCartFormatter
    {
        public string GenerateShoppingCartXmlString(PayExPayment payExPayment)
        {
            var shoppingCart = new ShoppingCart();
            var lineItems = CartHelper.GetLineItems(payExPayment);
            shoppingCart.ShoppingCartItem = lineItems.Select(x => new ShoppingCartItem
            {
                Description = x.DisplayName,
                Quantity = x.Quantity.RoundToLong(),
                Value = x.ExtendedPrice.RoundToLong()
            }).ToArray();
            shoppingCart.Subtotal = payExPayment.Amount.RoundToLong();
            shoppingCart.CurrencyCode = payExPayment.Parent.Parent.BillingCurrency;

            return PayExXmlSerializer.Serialize(shoppingCart);
        }
    }
}
