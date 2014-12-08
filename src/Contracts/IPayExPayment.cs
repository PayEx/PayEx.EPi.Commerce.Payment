
using System;

namespace EPiServer.Business.Commerce.Payment.PayEx.Contracts
{
    internal interface IPayExPayment
    {
        string OrderNumber { get; set; }
        string ReturnUrl { get; set; }
        string ProductNumber { get; set; }
        string PayExOrderRef { get; set; }
        string Description { get; set; }
        string ClientUserAgent { get; set; }
        string ClientIpAddress { get; set; }
        string CustomerId { get; set; }
        string AgreementReference { get; set; }
        string CancelUrl { get; set; }
        DateTime Created { get; set; }
    }
}
