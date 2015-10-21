using System;
using Mediachase.Commerce.Orders;
using Mediachase.Commerce.Orders.Dto;
using PayEx.EPi.Commerce.Payment.Contracts;
using PayEx.EPi.Commerce.Payment.Models.Result;

namespace PayEx.EPi.Commerce.Payment.Models.PaymentMethods
{
    public abstract class PaymentMethod
    {
        public IPayExPayment Payment { get; set; }
        public bool IsCart => OrderGroup is Cart;
        public bool IsPurchaseOrder => OrderGroup is PurchaseOrder;
        public PurchaseOrder PurchaseOrder => OrderGroup as PurchaseOrder;
        public Cart Cart => OrderGroup as Cart;
        public PaymentMethodDto PaymentMethodDto { get; private set; }

        private int _orderGroupId;
        public int OrderGroupId
        {
            get
            {
                if (_orderGroupId <= 0)
                    _orderGroupId = OrderGroup.Id;
                return _orderGroupId;
            }
            set { _orderGroupId = value; }
        }

        public OrderGroup OrderGroup { get; set; }
        private string TransactionType { get; set; }

        public bool IsCapture => TransactionTypeEquals(Mediachase.Commerce.Orders.TransactionType.Capture);

        public bool IsCredit => TransactionTypeEquals(Mediachase.Commerce.Orders.TransactionType.Credit);

        public bool IsAuthorization => TransactionTypeEquals(Mediachase.Commerce.Orders.TransactionType.Authorization);

        public PaymentMethod()
        {
            
        }

        public PaymentMethod(Mediachase.Commerce.Orders.Payment payment)
        {
            OrderGroup = payment.Parent.Parent;
            TransactionType = payment.TransactionType;
            Payment = payment as PayExPayment;
            PaymentMethodDto = Mediachase.Commerce.Orders.Managers.PaymentManager.GetPaymentMethod(payment.PaymentMethodId);
        }

        public abstract string PaymentMethodCode { get; }
        public abstract string DefaultView { get; }
        public abstract bool RequireAddressUpdate { get; }
        public abstract bool IsDirectModel { get; }
        public abstract PurchaseOperation PurchaseOperation { get; }
        public abstract PaymentInitializeResult Initialize();
        public abstract PaymentCompleteResult Complete(string orderRef);
        public abstract bool Capture();
        public abstract bool Credit();
        public abstract Address GetAddressFromPayEx(TransactionResult transactionResult);

        private bool TransactionTypeEquals(TransactionType transactionType)
        {
            return TransactionType.Equals(transactionType.ToString(), StringComparison.OrdinalIgnoreCase);
        }
    }
}
