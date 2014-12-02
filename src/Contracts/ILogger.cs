using System;

namespace EPiServer.Business.Commerce.Payment.PayEx.Contracts
{
    public interface ILogger
    {
        void LogInfo(string message);
        void LogDebug(string message);
        void LogWarning(string message);
        void LogWarning(string message, Exception exception);
        void LogError(string message, Exception exception);
        void LogError(string message);
    }
}
