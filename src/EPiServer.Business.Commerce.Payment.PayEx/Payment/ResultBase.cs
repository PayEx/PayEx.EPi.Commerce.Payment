
namespace EPiServer.Business.Commerce.Payment.PayEx.Payment
{
    internal class ResultBase
    {
        public bool Success { get; private set; }
        public string ErrorCode { get; set; }
        public string Description { get; set; }

        public ResultBase(bool success)
        {
            Success = success;
        }
    }
}
