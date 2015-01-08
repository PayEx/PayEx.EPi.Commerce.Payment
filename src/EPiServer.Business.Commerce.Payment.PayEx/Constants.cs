
namespace EPiServer.Business.Commerce.Payment.PayEx
{
    public static class Constants
    {
        public static class Metadata
        {
            public static class Namespace
            {
                public const string Order = "Mediachase.Commerce.Orders";
                public const string User = "Mediachase.Commerce.Orders.User";
                public const string OrderGroup = "Mediachase.Commerce.Orders.System.OrderGroup";
            }

            public class OrderFormPayment
            {
                public const string ClassName = "OrderFormPayment";
            }

            public class Payment
            {
                public const string ClassName = "PayExPayment";
                public const string TableName = "OrderFormPaymentEx_PayExPayment";
                public const string OrderNumber = "OrderNumber";
                public const string PayExOrderRef = "PayExOrderRef";
                public const string Description = "Description";
                public const string ProductNumber = "ProductNumber";
                public const string ClientIpAddress = "ClientIpAddress";
                public const string ClientUserAgent = "ClientUserAgent";
                public const string CancelUrl = "CancelUrl";
                public const string ReturnUrl = "ReturnUrl";
                public const string CustomerId = "CustomerId";
                public const string AgreementReference = "AgreementReference";
            }

            public static class OrderForm
            {
                public const string ClassName = "OrderFormEx";
                public const string PaymentMethodCode = "PaymentMethodCode";
            }

            public class LineItem
            {
                public const string ClassName = "LineItemEx";
                public const string VatAmount = "LineItemVatAmount";
                public const string VatPercentage = "LineItemVatPercentage";
            }
        }
    }
}
