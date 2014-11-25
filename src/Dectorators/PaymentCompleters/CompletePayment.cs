using System;
using System.Collections.Generic;
using Epinova.PayExProvider.Commerce;
using Epinova.PayExProvider.Contracts;
using Epinova.PayExProvider.Models;
using Mediachase.Commerce.Orders;
using Mediachase.Commerce.Orders.Managers;
using Mediachase.Data.Provider;
using PaymentMethod = Epinova.PayExProvider.Models.PaymentMethods.PaymentMethod;

namespace Epinova.PayExProvider.Dectorators.PaymentCompleters
{
    public class CompletePayment : IPaymentCompleter
    {
        private IPaymentCompleter _paymentCompleter;
        private readonly IPaymentManager _paymentManager;
        private readonly ILogger _logger;

        public CompletePayment(IPaymentCompleter paymentCompleter, IPaymentManager paymentManager, ILogger logger)
        {
            _paymentCompleter = paymentCompleter;
            _paymentManager = paymentManager;
            _logger = logger;
        }

        public PaymentCompleteResult Complete(PaymentMethod currentPayment, string orderRef)
        {
            CompleteResult completeResult = _paymentManager.Complete(orderRef);
            if (completeResult.Error || string.IsNullOrWhiteSpace(completeResult.TransactionNumber))
                return new PaymentCompleteResult { ErrorCode = completeResult.ErrorCode };

            if (completeResult.GetTransactionDetails)
            {
                if (_paymentCompleter == null)
                    _paymentCompleter = new UpdateTransactionDetails(null, _paymentManager, _logger);
                _paymentCompleter = new UpdateTransactionDetails(_paymentCompleter, _paymentManager, _logger);
            }

            bool purchaseOrderCreated = CreatePurchaseOrder(currentPayment, completeResult);

            PaymentCompleteResult result = new PaymentCompleteResult {Success = true};
            if (_paymentCompleter != null)
                result = _paymentCompleter.Complete(currentPayment, orderRef);
            result.Success = purchaseOrderCreated && result.Success;
            return result;
        }

        private bool CreatePurchaseOrder(PaymentMethod currentPayment, CompleteResult completeResult)
        {
            try
            {
                using (TransactionScope scope = new TransactionScope())
                {
                    currentPayment.Cart.Status = CartStatus.PaymentComplete.ToString();

                    // Change status of payments to processed. 
                    // It must be done before execute workflow to ensure payments which should mark as processed.
                    // To avoid get errors when executed workflow.
                    foreach (OrderForm orderForm in currentPayment.Cart.OrderForms)
                    {
                        foreach (Mediachase.Commerce.Orders.Payment payment in orderForm.Payments)
                        {
                            if (payment != null)
                                PaymentStatusManager.ProcessPayment(payment);
                        }
                    }

                    // Execute CheckOutWorkflow with parameter to ignore running process payment activity again.
                    var isIgnoreProcessPayment = new Dictionary<string, object>();
                    isIgnoreProcessPayment.Add("IsIgnoreProcessPayment", true);
                    OrderGroupWorkflowManager.RunWorkflow(currentPayment.Cart, OrderGroupWorkflowManager.CartCheckOutWorkflowName, true,
                        isIgnoreProcessPayment);

                    currentPayment.Cart.OrderNumberMethod = c => currentPayment.Payment.OrderNumber;
                    ((Mediachase.Commerce.Orders.Payment)currentPayment.Payment).AuthorizationCode = completeResult.TransactionNumber;
                    ((Mediachase.Commerce.Orders.Payment)currentPayment.Payment).AcceptChanges();
                    Mediachase.Commerce.Orders.PurchaseOrder purchaseOrder = currentPayment.Cart.SaveAsPurchaseOrder();

                    currentPayment.Cart.Delete();
                    currentPayment.Cart.AcceptChanges();

                    purchaseOrder.OrderForms[0]["PaymentMethodCode"] = completeResult.PaymentMethod;
                    purchaseOrder.AcceptChanges();

                    scope.Complete();
                }
            }
            catch (Exception e)
            {
                _logger.LogError("Error in ProcessSuccessfulTransaction", e);
                return false;
            }
            return true;
        }
    }
}
