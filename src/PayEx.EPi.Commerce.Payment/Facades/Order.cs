using System;
using PayEx.EPi.Commerce.Payment.Contracts;
using PayEx.EPi.Commerce.Payment.Models;

namespace PayEx.EPi.Commerce.Payment.Facades
{
    internal class Order : IOrderFacade
    {
        private PxOrder.PxOrderSoapClient _client;

        private PxOrder.PxOrderSoapClient Client
        {
            get
            {
                if (_client == null)
                    _client = new PxOrder.PxOrderSoapClient();
                return _client;
            }
        }

        public string Initialize(long accountNumber, PaymentInformation payment, string hash)
        {
            return Client.Initialize8(accountNumber, payment.PurchaseOperation, payment.Price, payment.PriceArgList, payment.Currency, payment.Vat, payment.OrderId,
                                      payment.ProductNumber, payment.Description, payment.ClientIpAddress, payment.ClientIdentifier, payment.AdditionalValues,
                                      string.Empty, payment.ReturnUrl, payment.View, payment.AgreementRef, payment.CancelUrl, payment.ClientLanguage, hash);
        }

        public string AddOrderLine(long accountNumber, OrderLine orderLine, string hash)
        {
            return Client.AddSingleOrderLine2(accountNumber, orderLine.OrderRef, orderLine.ItemNumber, orderLine.Description, orderLine.Description2, orderLine.Description3, 
                orderLine.Description4, orderLine.Description5, orderLine.Quantity, orderLine.Amount, orderLine.VatAmount, orderLine.VatPercentage, hash);
        }

        public string AddOrderAddress(long accountNumber, PayExAddress address, string hash)
        {
            return Client.AddOrderAddress2(accountNumber, address.OrderRef, address.BillingAddress.FirstName,
                address.BillingAddress.LastName, address.BillingAddress.Line1,
                address.BillingAddress.Line2, address.BillingAddress.Line3, address.BillingAddress.PostCode,
                address.BillingAddress.City, address.BillingAddress.State, address.BillingAddress.Country,
                address.BillingAddress.CountryCode, address.BillingAddress.Email, address.BillingAddress.Phone,
                address.BillingAddress.Mobile, address.ShippingAddress.FirstName, address.ShippingAddress.LastName,
                address.ShippingAddress.Line1, address.ShippingAddress.Line2, address.ShippingAddress.Line3,
                address.ShippingAddress.PostCode, address.ShippingAddress.City, address.ShippingAddress.State,
                address.ShippingAddress.Country, address.ShippingAddress.CountryCode, address.ShippingAddress.Email,
                address.ShippingAddress.Phone, address.ShippingAddress.Mobile, hash);
        }

        public string PurchaseInvoiceSale(long accountNumber, string orderRef, CustomerDetails customerDetails, string hash)
        {
            return Client.PurchaseInvoiceSale(accountNumber, orderRef, customerDetails.SocialSecurityNumber, customerDetails.FirstName,
                customerDetails.LastName, customerDetails.StreetAddress,
                customerDetails.CoAddress, customerDetails.PostNumber, customerDetails.City, customerDetails.CountryCode, customerDetails.Email,
                customerDetails.MobilePhone, customerDetails.IpAddress, hash);
        }

        public string PurchasePartPaymentSale(long accountNumber, string orderRef, CustomerDetails customerDetails,
            string hash)
        {
            return Client.PurchasePartPaymentSale(accountNumber, orderRef, customerDetails.SocialSecurityNumber,
                customerDetails.FirstName, customerDetails.LastName,
                customerDetails.StreetAddress, customerDetails.CoAddress, customerDetails.PostNumber,
                customerDetails.City, customerDetails.CountryCode, customerDetails.Email,
                customerDetails.MobilePhone, customerDetails.IpAddress, hash);
        }

        public string Complete(long accountNumber, string orderRef, string hash)
        {
            return Client.Complete(accountNumber, orderRef, hash);
        }

        public string Capture(long accountNumber, int transactionNumber, long amount, string orderId, int vatAmount, string additionalValues, string hash)
        {
            return Client.Capture5(accountNumber, transactionNumber, amount, orderId, vatAmount, additionalValues, hash);
        }

        public string GetTransactionDetails(long accountNumber, int transactionNumber, string hash)
        {
            return Client.GetTransactionDetails2(accountNumber, transactionNumber, hash);
        }

        public string Credit(long accountNumber, int transactionNumber, long amount, string orderId, int vatAmount, string additionalValues, string hash)
        {
            return Client.Credit5(accountNumber, transactionNumber, amount, orderId, vatAmount, additionalValues, hash);
        }

        public string CreditOrderLine(long accountNumber, int transactionNumber, string itemNumber, string orderId, string hash)
        {
            return Client.CreditOrderLine3(accountNumber, transactionNumber, itemNumber, orderId, hash);
        }

        public string GetApprovedDeliveryAddress(long accountNumber, string orderRef, string hash)
        {
            return Client.GetApprovedDeliveryAddress(accountNumber, orderRef, hash);
        }

        public string FinalizeTransaction(long accountNumber, string orderRef, long amount, long vatAmount, string clientIpAddress, string hash)
        {
            return Client.FinalizeTransaction(accountNumber, orderRef, amount, vatAmount, clientIpAddress, hash);
        }

        public string GetAddressByPaymentMethod(long accountNumber, string paymentMethod, string ssn, string zipcode, string countryCode, string ipAddress, string hash)
        {
            return Client.GetAddressByPaymentMethod(accountNumber, paymentMethod, ssn, zipcode, countryCode, ipAddress,
                hash);
        }

        public string PurchaseFinancingInvoice(long accountNumber, string orderRef, string paymentMethod, CustomerDetails customerDetails, string hash)
        {
            return Client.PurchaseFinancingInvoice(accountNumber, orderRef, customerDetails.SocialSecurityNumber, customerDetails.FullName, 
                customerDetails.StreetAddress, customerDetails.CoAddress, customerDetails.PostNumber, customerDetails.City, customerDetails.CountryCode, paymentMethod, customerDetails.Email,
                customerDetails.MobilePhone, customerDetails.IpAddress, hash);
        }

    }
}
