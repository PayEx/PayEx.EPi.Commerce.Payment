
using System;
using Mediachase.Commerce.Orders;
using Mediachase.Commerce.Orders.Dto;

namespace Epinova.PayExProvider.Models.PaymentMethods
{
    public abstract class PaymentMethod
    {
        public PayExPayment Payment { get; set; }
        public bool IsCart { get { return OrderGroup is Cart; } }
        public bool IsPurchaseOrder { get { return OrderGroup is PurchaseOrder; } }
        public PurchaseOrder PurchaseOrder { get { return OrderGroup as PurchaseOrder; } }
        public Cart Cart { get { return OrderGroup as Cart; } }
        public PaymentMethodDto PaymentMethodDto { get; private set; }

        private OrderGroup OrderGroup { get; set; }
        private string TransactionType { get; set; }

        public bool IsCapture
        {
            get
            {
                return TransactionTypeEquals(Mediachase.Commerce.Orders.TransactionType.Capture);
            }
        }

        public bool IsCredit
        {
            get
            {
                return TransactionTypeEquals(Mediachase.Commerce.Orders.TransactionType.Credit);
            }
        }

        public bool IsAuthorization
        {
            get
            {
                return TransactionTypeEquals(Mediachase.Commerce.Orders.TransactionType.Authorization);
            }
        }

        public PaymentMethod(Mediachase.Commerce.Orders.Payment payment)
        {
            OrderGroup = payment.Parent.Parent;
            TransactionType = payment.TransactionType;
            Payment = payment as PayExPayment;
            PaymentMethodDto = Mediachase.Commerce.Orders.Managers.PaymentManager.GetPaymentMethod(payment.PaymentMethodId);
        }

        private bool TransactionTypeEquals(TransactionType transactionType)
        {
            return TransactionType.Equals(transactionType.ToString(), StringComparison.OrdinalIgnoreCase);
        }
    }
}
