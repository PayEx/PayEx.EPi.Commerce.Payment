using System;
using Mediachase.Commerce.Orders;

namespace Epinova.PayExProvider.Contracts.Commerce
{
    public interface IOrderNote
    {
        OrderNote Create(Guid customerId, string detail, string title, string type,
            int? lineItemId = null, int? orderNoteId = null);
    }
}
