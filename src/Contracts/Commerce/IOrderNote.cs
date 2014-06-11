using System;
using Mediachase.Commerce.Orders;

namespace Epinova.PayExProvider.Contracts.Commerce
{
    public interface IOrderNote
    {
        int? FindTransactionIdByTitle(OrderNoteCollection orderNoteCollection, string title);

        OrderNote Create(Guid customerId, string detail, string title, string type,
            int? lineItemId = null, int? orderNoteId = null);
    }
}
