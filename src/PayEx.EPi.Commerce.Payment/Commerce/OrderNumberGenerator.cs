using System;
using Mediachase.Commerce.Orders;
using PayEx.EPi.Commerce.Payment.Contracts.Commerce;

namespace PayEx.EPi.Commerce.Payment.Commerce
{
    internal class OrderNumberGenerator : IOrderNumberGenerator
    {
        public string Generate(Cart cart)
        {
            var num = new Random().Next(1000, 9999).ToString();
            return $"{cart.OrderGroupId}{num}";
        }
    }
}
