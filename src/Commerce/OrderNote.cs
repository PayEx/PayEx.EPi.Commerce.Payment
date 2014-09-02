using System;
using System.Collections.Generic;
using System.Linq;
using Epinova.PayExProvider.Contracts;
using Epinova.PayExProvider.Contracts.Commerce;
using Mediachase.Commerce.Orders;

namespace Epinova.PayExProvider.Commerce
{
    public class OrderNote : IOrderNote
    {
        private readonly ILogger _logger;

        public OrderNote(ILogger logger)
        {
            _logger = logger;
        }

        public int? FindTransactionIdByTitle(OrderNoteCollection orderNoteCollection, string title)
        {
            List<Mediachase.Commerce.Orders.OrderNote> orderNotes = orderNoteCollection.Where(o => o.Title.Equals(title, StringComparison.OrdinalIgnoreCase)).ToList();
            if (!orderNotes.Any())
            {
                _logger.LogError(string.Format("Could not find ordernote with title:{0}", title));
                return null;
            }

            Mediachase.Commerce.Orders.OrderNote orderNote = orderNotes.First();

            if (string.IsNullOrWhiteSpace(orderNote.Detail))
            {
                _logger.LogError(string.Format("Ordernote with OrderNoteId:{0} has no Details", orderNote.OrderNoteId));
                return null;
            }

            string[] detailValues = orderNote.Detail.Split(' ');
            if (detailValues.Length >= 2)
            {
                int transactionId;
                if (Int32.TryParse(detailValues[1], out transactionId))
                    return transactionId;
                _logger.LogError(string.Format("Could not parse transaction Id for order with OrderNoteId:{0}. Attempted to parse:{1}", orderNote.OrderNoteId, detailValues[1]));
            }
            else
            {
                _logger.LogError(string.Format("Detail for order with OrderNoteId:{0} has the incorrect format:{1}", orderNote.OrderNoteId, orderNote.Detail));
            }
            return null;
        }

        public Mediachase.Commerce.Orders.OrderNote Create(Guid customerId, string detail, string title, string type, int? lineItemId = null, int? orderNoteId = null)
        {
            return new Mediachase.Commerce.Orders.OrderNote()
            {
                Created = DateTime.Now,
                CustomerId = customerId,
                Detail = detail,
                LineItemId = lineItemId,
                OrderNoteId = orderNoteId,
                Title = title,
                Type = type
            };
        }
    }
}
