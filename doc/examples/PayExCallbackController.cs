public class PaymentCallbackController : Controller
{
    // The user is redirected to paymentcallback/user/orderRef={guid}&orderNumber={string}&confirmationUrl={url} after having successfully supplied their payment information in PayEx
    public RedirectResult User(string orderRef, string orderNumber, string confirmationUrl)
    {
        UrlBuilder confirmationUrlBuilder = new UrlBuilder(confirmationUrl);

        // Get the cart for the current user
        Cart cart = new CartHelper(Cart.DefaultName).Cart;

        PayExPayment payExPayment = GetPayExPayment(cart);

        // If the cart doesn't contain a PayExPayment, something has gone wrong
        if (payExPayment == null)
            return ErrorResult(confirmationUrlBuilder, null);

        // If the PayExPayment doesn't contain an OrderNumber, something has gone wrong
        if (string.IsNullOrWhiteSpace(payExPayment.OrderNumber))
            return ErrorResult(confirmationUrlBuilder, null);

        // If the PayExPayment OrderNumber doesn't equal the orderNumber from the queryString, something has gone wrong
        if (!payExPayment.OrderNumber.Equals(orderNumber))
            return ErrorResult(confirmationUrlBuilder, null);

        PayExPaymentGateway gateway = new PayExPaymentGateway();
        string transactionErrorCode;

        // Complete the PayEx payment
        bool processed = gateway.ProcessSuccessfulTransaction(payExPayment, orderNumber, orderRef, cart, out transactionErrorCode);

        bool created = false;
        if (processed) // If the payment was successfully completed, you can create a Purchase Order
            created = CreatePurchaseOrder(cart, payExPayment, orderNumber);

        if (!created)
            return ErrorResult(confirmationUrlBuilder, transactionErrorCode);

        // Redirect the user to an order confirmation page
        confirmationUrlBuilder.QueryCollection.Add(ParameterName.OrderNumber, orderNumber);
        return new RedirectResult(confirmationUrlBuilder.ToString());
    }

    // This action is used for transactional callbacks: http://www.payexpim.com/quick-guide/9-transaction-callback/
    public HttpStatusCodeResult Index(string orderRef, [Bind(Prefix = ParameterName.PayExTransactionNumber)] string transactionNumber, [Bind(Prefix = ParameterName.PayExTransactionRef)] string transactionRef)
    {
        // Check that the request comes from a valid PayEx IP: http://www.payexpim.com/quick-guide/9-transaction-callback/
        // The IP should not be hardcoded, this is only an example!
        if (Request.ServerVariables["REMOTE_ADDR"] != "82.115.146.170")
            return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

        if (string.IsNullOrEmpty(orderRef))
            return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

        string orderNumber;
        Cart cart = GetCartByOrderRef(orderRef, out orderNumber);
        if (cart == null || string.IsNullOrEmpty(orderNumber))
            return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

        PayExPayment payExPayment = GetPayExPayment(cart);

        PayExPaymentGateway gateway = new PayExPaymentGateway();
        string transactionErrorCode;

        // Complete the PayEx payment
        bool success = gateway.ProcessSuccessfulTransaction(payExPayment, orderNumber, orderRef, cart, out transactionErrorCode);
        if (success)
            return new HttpStatusCodeResult(HttpStatusCode.OK);
        else
        {
            // If an order has already been created, maybe you'd want to cancel it if the callback wasn't successful?   
        }

        return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
    }

    /// <summary>
    /// Returns a Cart based on the PayEx orderReference
    /// </summary>
    /// <param name="orderRef">PayEx orderReference</param>
    /// <param name="orderNumber">Order number</param>
    /// <returns>Cart</returns>
    private Cart GetCartByOrderRef(string orderRef, out string orderNumber)
    {
        var parameters = new OrderSearchParameters
        {
            SqlMetaWhereClause = "",
            SqlWhereClause = "OrderGroupId IN (SELECT OrderGroupId FROM OrderFormPayment WHERE TransactionType LIKE 'Authorization' AND Status LIKE 'Pending')"
        };
        var options = new OrderSearchOptions
        {
            Classes = new StringCollection { "ShoppingCart" },
            CacheResults = false,
            RecordsToRetrieve = 10000,
            Namespace = "Mediachase.Commerce.Orders"
        };

        var carts = OrderContext.Current.FindCarts(parameters, options);
        foreach (var cart in carts)
        {
            var payment = GetPayExPayment(cart);
            if (payment == null || string.IsNullOrWhiteSpace(payment.PayExOrderRef))
                continue;

            if (payment.PayExOrderRef.Equals(orderRef))
            {
                orderNumber = payment.OrderNumber;
                return cart;
            }
        }
        orderNumber = string.Empty;
        return null;
    }

    /// <summary>
    /// Creates an error URL containing detailed information about the error that occured
    /// </summary>
    /// <param name="confirmationUrlBuilder">UrlBuilder containing the confirmation URL</param>
    /// <param name="transactionErrorCode">Error code returned from PayEx</param>
    /// <returns>A RedirectResult containing the error URL</returns>
    private RedirectResult ErrorResult(UrlBuilder confirmationUrlBuilder, string transactionErrorCode)
    {
        confirmationUrlBuilder.QueryCollection.Add("error", "true");

        if (!string.IsNullOrWhiteSpace(transactionErrorCode))
        {
            if (transactionErrorCode.Equals("CardNotAcceptedForThisPurchase"))
                confirmationUrlBuilder.QueryCollection.Add("declined", "true");
        }

        return new RedirectResult(confirmationUrlBuilder.ToString());
    }

    /// <summary>
    /// Returns a PayExPayment for the given cart, if one exists
    /// </summary>
    /// <param name="cart">Cart</param>
    /// <returns>The PayExPayment gor the given cart</returns>
    private PayExPayment GetPayExPayment(Cart cart)
    {
        if (cart.OrderForms == null || cart.OrderForms.Count == 0 || cart.OrderForms[0].Payments == null || cart.OrderForms[0].Payments.Count == 0)
            return null;

        List<Payment> payments = cart.OrderForms[0].Payments.Where(p => p is PayExPayment).ToList();
        payments = PaymentTransactionTypeManager.GetResultingPaymentsByTransactionType(payments,
            TransactionType.Authorization).ToList();

        if (payments.Any())
            return payments.First() as PayExPayment;
        return null;
    }

    /// <summary>
    /// Creates a purchase order with the given order number and deletes the cart
    /// </summary>
    /// <param name="cart">Cart</param>
    /// <param name="payment">Payment</param>
    /// <param name="orderNumber">orderNumber</param>
    /// <returns>Boolean indicating success or failure</returns>
    private bool CreatePurchaseOrder(Cart cart, Payment payment, string orderNumber)
    {
        try
        {
            using (TransactionScope scope = new TransactionScope())
            {
                PaymentStatusManager.ProcessPayment(payment);

                cart.OrderNumberMethod = c => orderNumber;
                PurchaseOrder purchaseOrder = cart.SaveAsPurchaseOrder();

                cart.Delete();
                cart.AcceptChanges();

                purchaseOrder.AcceptChanges();
                scope.Complete();
            }
            return true;
        }
        catch (Exception e)
        {
            // Add your own logging
            return false;
        }
    }
}