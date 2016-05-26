using System;
using System.Collections;
using System.Threading;
using System.Web;
using EPiServer.Logging.Compatibility;
using Mediachase.Commerce.Orders;
using PayEx.EPi.Commerce.Payment.Contracts.Commerce;

namespace PayEx.EPi.Commerce.Payment.Commerce
{
    internal class CartActions : ICartActions
    {
        protected readonly ILog Log = LogManager.GetLogger(Constants.Logging.DefaultLoggerName);
        private const string CurrentCartKey = "CurrentCart";
        private const string CurrentContextKey = "CurrentContext";

        /// <summary>
        /// Uses parameterized thread to update the cart instance id otherwise will get an "workflow already existed" exception.
        /// Passes the cart and the current HttpContext as parameter in call back function to be able to update the instance id and also can update the HttpContext.Current if needed.
        /// </summary>
        /// <param name="cart">The cart to update.</param>
        /// <remarks>
        /// This method is used internal for payment methods which has redirect standard for processing payment e.g: PayPal, DIBS
        /// </remarks>
        public void UpdateCartInstanceId(Cart cart)
        {
            ParameterizedThreadStart threadStart = UpdateCartCallbackFunction;
            var thread = new Thread(threadStart);
            var cartInfo = new Hashtable();
            cartInfo[CurrentCartKey] = cart;
            cartInfo[CurrentContextKey] = HttpContext.Current;
            thread.Start(cartInfo);
            thread.Join();
        }

        /// <summary>
        /// Callback function for updating the cart. Before accept all changes of the cart, update the HttpContext.Current if it is null somehow.
        /// </summary>
        /// <param name="cartArgs">The cart agruments for updating.</param>
        private void UpdateCartCallbackFunction(object cartArgs)
        {
            Log.Info("Begin saving all the changes that have been done to the cart");
            var cartInfo = cartArgs as Hashtable;
            if (cartInfo == null || !cartInfo.ContainsKey(CurrentCartKey))
                return;

            var cart = cartInfo[CurrentCartKey] as Cart;
            if (cart == null)
            {
                Log.Error("Current cart could not be found, cannot save changes!");
                return;
            }

            Log.InfoFormat("Saving all the changes that have been done to the cart with ID:{0}", cart.OrderGroupId);
            cart.InstanceId = Guid.NewGuid();
            if (HttpContext.Current == null && cartInfo.ContainsKey(CurrentContextKey))
                HttpContext.Current = cartInfo[CurrentContextKey] as HttpContext;

            try
            {
                cart.AcceptChanges();
                Log.InfoFormat("Successfylly saved all the changes that have been done to the cart with ID:{0}", cart.OrderGroupId);
            }
            catch (Exception ex)
            {
                Log.Error("Could not update cart in PayEx provider", ex);
            }
        }
    }
}
