using System;
using EPiServer.Business.Commerce.Payment.PayEx.Contracts.Commerce;
using Mediachase.Commerce.Orders;

namespace EPiServer.Business.Commerce.Payment.PayEx.Commerce
{
    internal class OrderNumberGenerator : IOrderNumberGenerator
    {
        public string Generate(Cart cart)
        {
            string num = new Random().Next(1000, 9999).ToString();
            return string.Format("{0}{1}", cart.OrderGroupId, num);
        }
    }
}
