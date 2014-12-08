using System;
using EPiServer.Business.Commerce.Payment.PayEx.Contracts.Commerce;
using Mediachase.Commerce.Orders;

namespace EPiServer.Business.Commerce.Payment.PayEx.Commerce
{
    public class OrderNumberGenerator : IOrderNumberGenerator
    {
        public string GenerateOrderNumber(Cart cart)
        {
            string str = new Random().Next(1000, 9999).ToString();
            return string.Format("{0}{1}", cart.OrderGroupId, str);
        }
    }
}
