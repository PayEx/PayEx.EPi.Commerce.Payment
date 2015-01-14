using System;
using EPiServer.Business.Commerce.Payment.PayEx.Contracts;
using EPiServer.Business.Commerce.Payment.PayEx.Models.Result;
using Mediachase.Commerce.Orders;
using Mediachase.Commerce.Orders.Dto;

namespace EPiServer.Business.Commerce.Payment.PayEx.Models.PaymentMethods
{
    public abstract class PaymentMethod
    {
        public IPayExPayment Payment { get; set; }
        public bool IsCart { get { return OrderGroup is Cart; } }
        public bool IsPurchaseOrder { get { return OrderGroup is PurchaseOrder; } }
        public PurchaseOrder PurchaseOrder { get { return OrderGroup as PurchaseOrder; } }
        public Cart Cart { get { return OrderGroup as Cart; } }
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
