
using ICSharpCode.SharpZipLib.Core;

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
                public const string PayExPaymentClassName = "PayExPayment";
                public const string ExtendedPayExPaymentClassName = "ExtendedPayExPayment";
                public const string PayExPaymentTableName = "OrderFormPaymentEx_PayExPayment";
                public const string ExtendedPayExPaymentTableName = "OrderFormPaymentEx_ExtendedPayExPayment";
                public const string OrderNumber = "OrderNumber";
                public const string PayExOrderRef = "PayExOrderRef";
                public const string Description = "Description";
                public const string ProductNumber = "ProductNumber";
                public const string ClientIpAddress = "ClientIpAddress";
                public const string CancelUrl = "CancelUrl";
                public const string ReturnUrl = "ReturnUrl";
                public const string CustomerId = "CustomerId";
                public const string SocialSecurityNumber = "SocialSecurityNumber";
                public const string FirstName = "FirstName";
                public const string LastName = "LastName";
                public const string StreetAddress = "StreetAddress";
                public const string CoAddress = "CoAddress";
                public const string City = "City";
                public const string CountryCode = "CountryCode";
                public const string Email = "Email";
                public const string MobilePhone = "MobilePhone";
                public const string PostNumber = "PostNumber";
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

        public static class Payment
        {
            public static class CreditCard
            {
                public const string SystemKeyword = "PayEx_CreditCard";
                public const string Name = "PayEx CreditCard";
            }

            public static class InvoiceLedger
            {
                public const string SystemKeyword = "PayEx_InvoiceLedger";
                public const string Name = "PayEx Invoice Ledger";
            }

            public static class Invoice
            {
                public const string SystemKeyword = "PayEx_Invoice";
                public const string Name = "PayEx Invoice 2.0";
            }

            public static class DirectDebit
            {
                public const string SystemKeyword = "PayEx_DirectDebit";
                public const string Name = "PayEx Direct Debit";
            }

            public static class PartPayment
            {
                public const string SystemKeyword = "PayEx_PartPayment";
                public const string Name = "PayEx Part Payment";
            }

            public static class PayPal
            {
                public const string SystemKeyword = "PayEx_Paypal";
                public const string Name = "PayEx Paypal";
            }

            public static class Giftcard
            {
                public const string SystemKeyword = "PayEx_GiftCard";
                public const string Name = "PayEx GiftCard";
            }
        }
    }
}
