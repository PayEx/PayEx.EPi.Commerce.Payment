﻿using System.Security.Cryptography;
using System.Text;
using PayEx.EPi.Commerce.Payment.Contracts;
using PayEx.EPi.Commerce.Payment.Models;

namespace PayEx.EPi.Commerce.Payment.Payment
{
    internal class Hash : IHasher
    {
        public string Create(long accountNumber, PaymentInformation payment, string encryptionKey)
        {
            var stringToHash = string.Concat(accountNumber, payment.PurchaseOperation, payment.Price, payment.PriceArgList, payment.Currency, payment.Vat, payment.OrderId, payment.ProductNumber,
                                payment.Description, payment.ClientIpAddress, payment.ClientIdentifier, payment.AdditionalValues, payment.ReturnUrl, payment.View,
                                payment.AgreementRef, payment.CancelUrl, payment.ClientLanguage, encryptionKey);

            return CreateHash(stringToHash);
        }

        public string Create(long accountNumber, OrderLine orderLine, string encryptionKey)
        {
            var stringToHash = string.Concat(accountNumber, orderLine.OrderRef, orderLine.ItemNumber, orderLine.Description, orderLine.Description2, orderLine.Description3,
                orderLine.Description4, orderLine.Description5, orderLine.Quantity, orderLine.Amount, orderLine.VatAmount, orderLine.VatPercentage, encryptionKey);

            return CreateHash(stringToHash);
        }

        public string Create(long accountNumber, PayExAddress address, string encryptionKey)
        {
            var stringToHash = string.Concat(accountNumber, address.OrderRef, address.BillingAddress.FirstName,
                address.BillingAddress.LastName, address.BillingAddress.Line1,
                address.BillingAddress.Line2, address.BillingAddress.Line3, address.BillingAddress.PostCode,
                address.BillingAddress.City, address.BillingAddress.State, address.BillingAddress.Country,
                address.BillingAddress.CountryCode, address.BillingAddress.Email, address.BillingAddress.Phone,
                address.BillingAddress.Mobile, address.ShippingAddress.FirstName, address.ShippingAddress.LastName,
                address.ShippingAddress.Line1, address.ShippingAddress.Line2, address.ShippingAddress.Line3,
                address.ShippingAddress.PostCode, address.ShippingAddress.City, address.ShippingAddress.State,
                address.ShippingAddress.Country, address.ShippingAddress.CountryCode, address.ShippingAddress.Email,
                address.ShippingAddress.Phone, address.ShippingAddress.Mobile, encryptionKey);

            return CreateHash(stringToHash);
        }

        public string Create(long accountNumber, int transactionNumber, string encryptionKey)
        {
            var stringToHash = string.Concat(accountNumber, transactionNumber, encryptionKey);
            return CreateHash(stringToHash);
        }

        public string Create(long accountNumber, string orderRef, string encryptionKey)
        {
            var stringToHash = string.Concat(accountNumber, orderRef, encryptionKey);
            return CreateHash(stringToHash);
        }

        public string Create(long accountNumber, string socialSecurityNumber, string countryCode, string encryptionKey)
        {
            var stringToHash = string.Concat(accountNumber, countryCode, socialSecurityNumber, encryptionKey);
            return CreateHash(stringToHash);
        }

        public string Create(long accountNumber, string orderRef, CustomerDetails customerDetails, string encryptionKey)
        {
            var stringToHash = string.Concat(accountNumber, orderRef, customerDetails.SocialSecurityNumber, customerDetails.FirstName, customerDetails.LastName, customerDetails.StreetAddress, customerDetails.CoAddress,
                customerDetails.PostNumber, customerDetails.City, customerDetails.CountryCode, customerDetails.Email, customerDetails.MobilePhone, customerDetails.IpAddress, encryptionKey);
            return CreateHash(stringToHash);
        }

        public string Create(long accountNumber, string orderRef, string paymentMethod, CustomerDetails customerDetails, string encryptionKey)
        {
            var stringToHash = string.Concat(accountNumber, orderRef, customerDetails.SocialSecurityNumber, customerDetails.FullName, customerDetails.StreetAddress, customerDetails.CoAddress,
                customerDetails.PostNumber, customerDetails.City, customerDetails.CountryCode, paymentMethod, customerDetails.Email, customerDetails.MobilePhone, customerDetails.IpAddress, encryptionKey);
            return CreateHash(stringToHash);
        }

        public string Create(long accountNumber, int transactionNumber, long amount, string orderId, int vatAmount, string additionalValues, string encryptionKey)
        {
            var stringToHash = string.Concat(accountNumber, transactionNumber, amount, orderId, vatAmount, additionalValues, encryptionKey);
            return CreateHash(stringToHash);
        }

        public string Create(long accountNumber, int transactionNumber, string itemNumber, string orderId, string encryptionKey)
        {
            var stringToHash = string.Concat(accountNumber, transactionNumber, itemNumber, orderId, encryptionKey);
            return CreateHash(stringToHash);
        }

        public string Create(long accountNumber, string orderRef, long amount, long vatAmount, string clientIpAddress, string encryptionKey)
        {
            var stringToHash = string.Concat(accountNumber, orderRef, amount, vatAmount, clientIpAddress, encryptionKey);
            return CreateHash(stringToHash);
        }

        public string Create(long accountNumber, string paymentMethod, string ssn, string zipcode, string countryCode, string ipAddress, string encryptionKey)
        {
            var stringToHash = string.Concat(accountNumber, paymentMethod, ssn, zipcode, countryCode, ipAddress, encryptionKey);
            return CreateHash(stringToHash);
        }

        private string CreateHash(string stringToHash)
        {
            var hash = new MD5CryptoServiceProvider();
            var data = hash.ComputeHash(Encoding.Default.GetBytes(stringToHash));
            var stringBuilder = new StringBuilder();

            for (int i = 0; i < data.Length; i++)
            {
                stringBuilder.Append(data[i].ToString("x2"));
            }

            return stringBuilder.ToString();
        }

    }
}
